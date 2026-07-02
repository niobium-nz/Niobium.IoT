using Cod.IoT.Networking;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Hub
{
    public class HubService(IDevice device, IConfigurationProvider configuration, INetworkManager networkManager) : GenericService, IHubService
    {
        private Thread? worker;

        public bool IsConnected => device != null && device.Status == DeviceConnectionStatus.Connected;

        public bool IsConnecting => device != null && device.Status == DeviceConnectionStatus.Disconnected_Retrying;

        public override int ID => Constants.HubServiceID;

        public bool AutoConnect { get; set; } = false;

        public void Connect()
        {
            if (worker == null)
            {
                worker = new Thread(EnsureConnection);
                worker.Start();
            }
        }

        protected virtual async void EnsureConnection()
        {
            do
            {
                if (IsConnected || IsConnecting)
                {
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                if (!networkManager.IsEstablished)
                {
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                string host = configuration.GetAsString(Constants.ConfigHubHost);
                if (string.IsNullOrEmpty(host))
                {
                    Thread.Sleep(Constants.HubConnectionRetryInterval);
                    continue;
                }

                Logger.LogInformation($"Device is connecting to {host}...");
                await OpenAsync(host);
                Thread.Sleep(Constants.HubConnectionRetryInterval);
            } while (AutoConnect);
        }

        protected virtual async Task OpenAsync(string assignedIoTHub)
        {
            try
            {
                await device.ConnectAsync();
                Logger.LogInformation("IoT connection established.");
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "IoT connection fail on open.");
            }
        }

        protected async override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AutoConnect = false;
                worker?.Join(Constants.NetworkWaitInterval);
                worker = null;

                if (device != null)
                {
                    try
                    {
                        await device.DisposeAsync();
                    }
                    catch
                    {
                    }
                }
            }

            base.Dispose(disposing);
        }
    }
}