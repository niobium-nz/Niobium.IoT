using System.Collections.Concurrent;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Hub
{
    public class X509IoTHubDevice : IDevice
    {
        private static readonly TimeSpan busyInterval = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan idleInterval = TimeSpan.FromMilliseconds(1000);
        private string primaryCertificatePath;
        private string primaryCertificatePassword;
        private string secondaryCertificatePath;
        private string secondaryCertificatePassword;
        private readonly string databaseFilePath;
        private X509Certificate2 currentActiveCertificate;
        private readonly IConfigurationProvider configuration;
        private readonly IAsyncCommandService commandService;
        private readonly ILogger logger;
        private CancellationTokenSource? sendingTaskCancellation;
        private CancellationTokenSource? connectCancellationTokenSource;
        private Task? sendingTask;
        private DateTimeOffset lastCPR = DateTimeOffset.MinValue;
        private static readonly TimeSpan cprInterval = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan cprEventsThreshold = TimeSpan.FromMinutes(1);
        private bool disposed;

        protected ConcurrentQueue<ITimestampable> Events { get; set; } = new();

        protected DeviceClient? DeviceClient { get; private set; }

        protected string DeviceID => this.currentActiveCertificate.GetNameInfo(X509NameType.SimpleName, false);

        public DeviceConnectionStatus Status { get; private set; }

        public X509IoTHubDevice(IConfigurationProvider configuration, IAsyncCommandService commandService, ILogger<X509IoTHubDevice> logger)
        {
            this.configuration = configuration;
            this.commandService = commandService;
            this.logger = logger;

            this.primaryCertificatePath = configuration.GetSetting<string>(Constants.ConfigPrimaryCertificatePath,
                check: v => File.Exists(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigPrimaryCertificatePath} does not exist: {v}");

            this.secondaryCertificatePath = configuration.GetSetting<string>(Constants.ConfigSecondaryCertificatePath,
                check: v => File.Exists(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigSecondaryCertificatePath} does not exist: {v}");

            this.primaryCertificatePassword = configuration.GetSetting<string>(Constants.ConfigPrimaryCertificatePassword,
                check: v => !string.IsNullOrWhiteSpace(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigPrimaryCertificatePassword} must be configured.");

            this.secondaryCertificatePassword = configuration.GetSetting<string>(Constants.ConfigSecondaryCertificatePassword,
                check: v => !string.IsNullOrWhiteSpace(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigSecondaryCertificatePassword} must be configured.");

            this.secondaryCertificatePassword = configuration.GetSetting<string>(Constants.ConfigSecondaryCertificatePassword,
                check: v => !string.IsNullOrWhiteSpace(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigSecondaryCertificatePassword} must be configured.");

            this.databaseFilePath = configuration.GetSetting<string>(Constants.ConfigDatabaseFilePath,
                check: v => !string.IsNullOrWhiteSpace(v),
                exceptionMessageOnFailedCheck: v => $"{Constants.ConfigDatabaseFilePath} must be configured.");

            this.currentActiveCertificate = LoadCertificate(this.primaryCertificatePath, this.primaryCertificatePassword);
        }

        public void Send(ITimestampable data)
        {
            this.Events.Enqueue(data);
            this.SaveToDatabase();
        }

        public virtual async Task ReportTwinsAsync(IDictionary<string, object> properties, CancellationToken cancellationToken)
        {
            var twins = new TwinCollection();
            foreach (var key in properties.Keys)
            {
                twins[key] = properties[key];
            }

            if (this.Status == DeviceConnectionStatus.Connected && this.DeviceClient != null)
            {
                await this.DeviceClient.UpdateReportedPropertiesAsync(twins, cancellationToken);
            }
        }

        public virtual async Task ConnectAsync()
        {
            if (this.ShouldClientBeInitialized)
            {
                this.logger.LogDebug($"Attempting to initialize the device client instance, current status={this.Status}");
                Status = DeviceConnectionStatus.Disconnected_Retrying;

                var assignedHub = this.configuration.GetSetting<string>(Constants.ConfigHubHost,
                    check: v => !string.IsNullOrWhiteSpace(v),
                    exceptionMessageOnFailedCheck: v => $"{Constants.ConfigHubHost} must be configured.");

                try
                {
                    await this.DisconnectAsync();
                    var auth = new DeviceAuthenticationWithX509Certificate(this.DeviceID, currentActiveCertificate);
                    this.DeviceClient = DeviceClient.Create(assignedHub, auth, TransportType.Mqtt_WebSocket_Only);
                    this.DeviceClient.SetConnectionStatusChangesHandler(this.ConnectionStatusChangeHandler);

                    this.connectCancellationTokenSource = new CancellationTokenSource();
                    await this.DeviceClient.OpenAsync(this.connectCancellationTokenSource.Token);
                    this.logger.LogDebug("Device client instance opened.");
                }
                catch (UnauthorizedException)
                {
                    this.SwapSecondaryCredentials();
                    await Task.Delay(10000);
                    await this.ConnectAsync();
                }
                catch (Exception e)
                {
                    this.logger.LogError(e, e.Message);
                }
            }
        }

        protected async Task DisconnectAsync()
        {
            await this.CancelTaskAsync(this.connectCancellationTokenSource);
            await this.CancelTaskAsync(this.sendingTaskCancellation, this.sendingTask);
            this.sendingTask = null;
            this.sendingTaskCancellation = null;

            // If the device client instance has been previously initialized, then dispose it.
            if (this.DeviceClient != null)
            {
                this.logger.LogDebug($"Previous deivce client in place, disposing...");
                using (this.DeviceClient)
                {
                    if (this.Status == DeviceConnectionStatus.Connected)
                    {
                        try
                        {
                            await this.UnregisterDirectMethodsAsync(CancellationToken.None);
                            await this.DeviceClient.SetDesiredPropertyUpdateCallbackAsync(null, null);
                            await this.DeviceClient.SetReceiveMessageHandlerAsync(null, null);
                            await this.DeviceClient.CloseAsync();
                        }
                        catch (Exception e)
                        {
                            this.logger.LogError(e, e.Message);
                        }
                    }
                }

                this.logger.LogDebug($"Previous deivce client has been disposed.");
                this.DeviceClient = null;
            }
        }

        protected async virtual Task RegisterDirectMethodsAsync(CancellationToken cancellationToken)
        {
            await this.DeviceClient!.SetMethodHandlerAsync("Execute", this.DirectMethodAsync, null, cancellationToken);
        }

        protected virtual async Task UnregisterDirectMethodsAsync(CancellationToken cancellationToken)
        {
            await this.DeviceClient!.SetMethodHandlerAsync("Execute", null, null, cancellationToken);
        }

        protected virtual async Task<MethodResponse> DirectMethodAsync(MethodRequest methodRequest, object userContext)
        {
            try
            {
                var result = await this.commandService.ExecuteAsync(methodRequest.DataAsJson);
                var json = JSON.Instance.Serialize(result);
                return new MethodResponse(Encoding.UTF8.GetBytes(json), result.S);
            }
            catch (Exception e)
            {
                var error = JSON.Instance.Serialize(new
                {
                    Type = e.GetType().FullName,
                    Message = e.Message,
                    Stack = e.StackTrace,
                });
                return new MethodResponse(Encoding.UTF8.GetBytes(error), 500);
            }
        }

        protected virtual void SaveToDatabase()
        {
            try
            {
                var records = new List<ITimestampable>(this.Events);
                var json = JSON.Instance.Serialize(records);
                File.WriteAllText(databaseFilePath, json, Encoding.UTF8);
            }
            catch
            {
            }
        }

        protected virtual void LoadFromDatabase()
        {
            var json = File.ReadAllText(databaseFilePath, Encoding.UTF8);
            if (!String.IsNullOrWhiteSpace(json))
            {
                try
                {
                    var records = this.DeserializeDatabaseRecords(json);
                    if (records != null && records.Any())
                    {
                        foreach (var item in records)
                        {
                            this.Events.Enqueue(item);
                        }
                    }
                }
                catch (Exception)
                {
                    this.SaveToDatabase();
                }
            }
        }

        protected IEnumerable<ITimestampable> DeserializeDatabaseRecords(string json) => JSON.Instance.Deserialize<DeviceTelemetry[]>(json);

        private async Task SendCoreAsync()
        {
            while (this.sendingTaskCancellation != null && !this.sendingTaskCancellation.Token.IsCancellationRequested)
            {
                if (this.Status == DeviceConnectionStatus.Connected && !this.Events.IsEmpty && this.DeviceClient != null)
                {
                    var sending = new List<ITimestampable>();

                    try
                    {
                        while (this.Events.TryDequeue(out var data))
                        {
                            data.Timestamp = DateTimeOffset.UtcNow;
                            sending.Add(data);
                        }

                        var json = JSON.Instance.Serialize(sending);
                        using var message = new Message(Encoding.UTF8.GetBytes(json))
                        {
                            ContentEncoding = "utf-8",
                            ContentType = "application/json",
                        };

                        await this.DeviceClient.SendEventAsync(message, this.sendingTaskCancellation.Token).ConfigureAwait(false);
                        await this.OnSentAsync(this, sending, this.sendingTaskCancellation.Token).ConfigureAwait(false);
                        this.SaveToDatabase();
                        sending.Clear();
                        continue;
                    }
                    catch (IotHubException ex) when (ex.IsTransient)
                    {
                        // Inspect the exception to figure out if operation should be retried, or if user-input is required.
                        this.logger.LogWarning($"An IotHubException was caught, but will try to recover and retry: {ex}");
                    }
                    catch (Exception ex) when (ex.IsNetworkExceptionChain())
                    {
                        this.logger.LogWarning($"A network related exception was caught, but will try to recover and retry: {ex}");
                    }
                    catch (Exception ex)
                    {
                        this.logger.LogError($"Unexpected error: {ex}");
                    }
                    finally
                    {
                        if (sending.Count > 0)
                        {
                            await this.OnSendFailedAsync(this, sending, this.sendingTaskCancellation.Token).ConfigureAwait(false);

                            foreach (var item in sending)
                            {
                                this.Events.Enqueue(item);
                            }
                            sending.Clear();
                            this.SaveToDatabase();
                        }

                        // wait and retry
                        await Task.Delay(busyInterval, this.sendingTaskCancellation.Token).ConfigureAwait(false);
                    }
                }
                else
                {
                    await Task.Delay(idleInterval, this.sendingTaskCancellation.Token).ConfigureAwait(false);
                }
            }
        }

        protected async virtual Task<bool> OnReceivedAsync(CloudToDeviceMessage message, Message rawMessage)
        {
            try
            {
                var result = await this.commandService.ExecuteAsync(message.JSONBody);
                return result.S >= 200 && result.S < 400;
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
                return false;
            }
        }

        protected virtual Task OnSentAsync(object sender, List<ITimestampable> messages, CancellationToken cancellationToken) => Task.CompletedTask;

        protected virtual Task OnSendFailedAsync(object sender, List<ITimestampable> messages, CancellationToken cancellationToken) => Task.CompletedTask;

        protected virtual async Task ReceiveAsync(Message receivedMessage, object _)
        {
            using (receivedMessage)
            {
                var success = false;
                var msgbody = String.Empty;
                try
                {
                    this.logger.LogDebug($"{DateTime.Now}> C2D message callback - message received with Id={receivedMessage.MessageId}.");
                    using var reader = new StreamReader(receivedMessage.BodyStream, Encoding.UTF8);
                    msgbody = await reader.ReadToEndAsync();
                    this.logger.LogDebug($"C2D message: {msgbody}");
                    success = await this.OnReceivedAsync(new CloudToDeviceMessage
                    {
                        JSONBody = msgbody,
                        CorrelationID = receivedMessage.CorrelationId,
                        Created = receivedMessage.CreationTimeUtc.Year > 2020 ? new DateTimeOffset(receivedMessage.CreationTimeUtc) : DateTimeOffset.UtcNow,
                        DeliveryCount = receivedMessage.DeliveryCount,
                        Enqueued = receivedMessage.EnqueuedTimeUtc.Year > 2020 ? new DateTimeOffset(receivedMessage.EnqueuedTimeUtc) : DateTimeOffset.UtcNow,
                        Expires = receivedMessage.Properties.ContainsKey(nameof(CloudToDeviceMessage.Expires)) ? DateTimeOffset.Parse(receivedMessage.Properties[nameof(CloudToDeviceMessage.Expires)]) : DateTimeOffset.MaxValue,
                        Valids = receivedMessage.Properties.ContainsKey(nameof(CloudToDeviceMessage.Valids)) ? DateTimeOffset.Parse(receivedMessage.Properties[nameof(CloudToDeviceMessage.Valids)]) : DateTimeOffset.MinValue,
                    }, receivedMessage);
                }
                finally
                {
                    if (success)
                    {
                        this.logger.LogDebug($"{DateTime.Now}> Completed C2D message with Id={receivedMessage.MessageId}.");
                        await this.CompleteAsync(receivedMessage);
                    }
                    else
                    {
                        this.logger.LogError($"Rejected C2D message with Id={receivedMessage.MessageId}: {msgbody}");
                        await this.RejectAsync(receivedMessage);
                    }
                }

            }
        }

        protected virtual async Task CompleteAsync(Message message)
        {
            if (this.DeviceClient != null)
            {
                await this.DeviceClient.CompleteAsync(message);
            }
        }

        protected virtual async Task RejectAsync(Message message)
        {
            if (this.DeviceClient != null)
            {
                await this.DeviceClient.RejectAsync(message);
            }
        }

        protected virtual async void ConnectionStatusChangeHandler(ConnectionStatus status, ConnectionStatusChangeReason reason)
        {
            this.logger.LogDebug($"Connection status changed: status={status}, reason={reason}");
            this.Status = (DeviceConnectionStatus)(int)status;

            if (this.Status == DeviceConnectionStatus.Connected)
            {
                if (this.sendingTaskCancellation == null)
                {
                    this.sendingTaskCancellation = new CancellationTokenSource();
                    this.sendingTask = Task.Run(this.SendCoreAsync, this.sendingTaskCancellation.Token);

                    await this.DeviceClient!.SetDesiredPropertyUpdateCallbackAsync(this.OnDesiredPropertyChangedAsync, null, this.sendingTaskCancellation.Token);
                    await this.DeviceClient!.SetReceiveMessageHandlerAsync(this.ReceiveAsync, this.DeviceClient, this.sendingTaskCancellation.Token);
                    await this.RegisterDirectMethodsAsync(this.sendingTaskCancellation.Token);
                }

                var twin = await this.DeviceClient!.GetTwinAsync(this.sendingTaskCancellation.Token);
                await this.OnDesiredPropertyChangedAsync(twin.Properties.Desired, null);
                await this.ReportTwinsAsync(CancellationToken.None);
                this.logger.LogDebug("### The DeviceClient is CONNECTED; all operations will be carried out as normal.");
            }
        }

        public async Task ReportTwinsAsync(CancellationToken cancellationToken)
        {
            Dictionary<string, object> report = [];
            foreach (string key in this.configuration.Keys)
            {
                report.Add(key, this.configuration.GetAsObject(key));
            }

            try
            {
                await ReportTwinsAsync(report, Constants.DeviceTwinsReportMaxRetry, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Initial twins reporting has failed.");
            }
        }

        protected virtual async Task<bool> ReportTwinsAsync(IDictionary<string, object> properties, ushort retry, CancellationToken cancellationToken)
        {
            if (this.Status == DeviceConnectionStatus.Connected && this.DeviceClient != null)
            {
                try
                {
                    TwinCollection report = new();
                    foreach (var key in properties.Keys)
                    {
                        report[key] = properties[key];
                    }

                    await DeviceClient.UpdateReportedPropertiesAsync(report, cancellationToken);
                    return true;
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "IoT twins report failed.");
                }
            }

            if (retry <= 0)
            {
                return false;
            }

            await Task.Delay(Constants.DeviceTwinsReportRetryInterval);
            return await ReportTwinsAsync(properties, --retry, cancellationToken);
        }

        protected virtual async Task OnDesiredPropertyChangedAsync(TwinCollection desiredProperties, object? userContext)
        {
            var lastTwinVersion = configuration.Get<long>(Constants.ConfigLastTwinVersion);
            if (lastTwinVersion >= desiredProperties.Version)
            {
                // do not proceed on any update that's older on its version
                return;
            }

            var properties = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> desiredProperty in desiredProperties)
            {
                properties.Add(desiredProperty.Key, desiredProperty.Value);
            }

            if (properties.Count > 0)
            {
                await this.OnDesiredPropertyUpdated(properties);
            }

            configuration.Set(Constants.ConfigLastTwinVersion, desiredProperties.Version);
            configuration.Save();
        }

        protected virtual Task OnDesiredPropertyUpdated(IDictionary<string, object> properties)
        {
            foreach (string key in properties.Keys)
            {
                if (key[0] != '$')
                {
                    configuration.Set(key, properties[key]);
                }
            }

            return Task.CompletedTask;
        }

        private async Task CancelTaskAsync(CancellationTokenSource? source, Task? task = null)
        {
            try
            {
                if (source != null)
                {
                    using (source)
                    {
                        if (!source.IsCancellationRequested)
                        {
                            this.logger.LogDebug($"Canceling task...");
                            source.Cancel();
                        }

                        if (task != null)
                        {
                            try
                            {
                                await Task.WhenAll([task]);
                            }
                            catch (TaskCanceledException)
                            {
                            }

                        }
                    }

                    this.logger.LogDebug($"Task cancelled.");
                }
            }
            catch (Exception e)
            {
                this.logger.LogError(e, e.Message);
            }
        }

        private void SwapSecondaryCredentials()
        {
            if (!String.IsNullOrWhiteSpace(this.secondaryCertificatePath) && !String.IsNullOrWhiteSpace(this.secondaryCertificatePassword))
            {
                var swapPfx = this.primaryCertificatePath;
                var swapPassword = this.primaryCertificatePassword;

                this.logger.LogWarning($"The current connection string is invalid. Trying another.");
                this.primaryCertificatePath = this.secondaryCertificatePath;
                this.primaryCertificatePassword = this.secondaryCertificatePassword;
                this.secondaryCertificatePath = swapPfx;
                this.secondaryCertificatePassword = swapPassword;

                currentActiveCertificate = LoadCertificate(this.primaryCertificatePath, this.primaryCertificatePassword);
            }
        }

        private static X509Certificate2 LoadCertificate(string pfxCertificatePath, string pfxCertificatePassword)
        {
            var certificateCollection = new X509Certificate2Collection();
            certificateCollection.Import(pfxCertificatePath, pfxCertificatePassword, X509KeyStorageFlags.UserKeySet);

            X509Certificate2? certificate = null;
            foreach (var element in certificateCollection)
            {
                if (certificate == null && element.HasPrivateKey)
                {
                    certificate = element;
                }
                else
                {
                    element.Dispose();
                }
            }

            return certificate ?? throw new FileNotFoundException($"{pfxCertificatePath} did not contain any certificate with a private key.");
        }

        // If the client reports Connected status, it is already in operational state.
        // If the client reports Disconnected_retrying status, it is trying to recover its connection.
        // If the client reports Disconnected status, you will need to dispose and recreate the client.
        // If the client reports Disabled status, you will need to dispose and recreate the client.
        private bool ShouldClientBeInitialized
        {
            get
            {
                if (this.Status == DeviceConnectionStatus.Disconnected || this.Status == DeviceConnectionStatus.Disabled)
                {
                    return true;
                }

                var now = DateTimeOffset.UtcNow;
                if (!this.Events.IsEmpty && now - lastCPR > cprInterval)
                {
                    var earliestEventTimestamp = this.Events.Min(e => e.Timestamp);
                    if (now - earliestEventTimestamp > cprEventsThreshold)
                    {
                        // Something went wrong, network connection has disconnected in fact without us getting notified
                        this.Status = DeviceConnectionStatus.Disconnected;
                        lastCPR = DateTimeOffset.UtcNow;
                        return true;
                    }
                }

                return false;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!this.disposed)
            {
                await this.DisposeAsync(true);
            }

            this.disposed = true;
        }


        protected virtual async ValueTask DisposeAsync(bool disposing)
        {
            if (disposing)
            {
                await this.DisconnectAsync();
                SaveToDatabase();
            }
        }
    }
}
