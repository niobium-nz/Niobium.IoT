using Cod.IoT.Networking;
using Microsoft.Extensions.Logging;
using nanoFramework.Azure.Devices.Client;
using nanoFramework.Azure.Devices.Shared;
using nanoFramework.Json;
using nanoFramework.M2Mqtt;
using nanoFramework.M2Mqtt.Messages;
using System;
using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace Cod.IoT.Hub
{
    public class HubService : GenericService, IHubService
    {
        private Thread worker;
        private DeviceClient deviceClient;

        public bool IsConnected => deviceClient != null && deviceClient.IsConnected;

        public override int ID => Constants.HubServiceID;

        public bool AutoConnect { get; set; } = false;

        public event CommandArrivedEventHandler CommandArrived;

        public event EventHandler ConnectionChanged;

        public void Connect()
        {
            if (worker == null)
            {
                worker = new Thread(EnsureConnection);
                worker.Start();
            }
        }

        protected virtual void EnsureConnection()
        {
            ushort failures = 0;

            do
            {
                if (IsConnected)
                {
                    failures = 0;
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                INetworkManager networkManager = (INetworkManager)GetService(Constants.NetworkManagerID);
                if (!networkManager.IsEstablished)
                {
                    failures = 0;
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                IConfigurationProvider configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
                string host = configuration.GetAsString(Constants.ConfigHubHost);
                string key = configuration.GetAsString(Constants.ConfigHubKey);
                string deviceID = configuration.GetAsString(Constants.ConfigDeviceID);

                if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(key) || string.IsNullOrEmpty(deviceID))
                {
                    failures = 0;
                    Thread.Sleep(Constants.HubConnectionRetryInterval);
                    continue;
                }

                Logger.LogInformation($"Device {deviceID} is connecting to {host}...");
                bool success = Open(host, deviceID, key);
                if (success)
                {
                    FetchIniaialTwins();
                    ReportTwins();
                    Logger.LogInformation("Hub initialized successfully.");
                }
                else
                {
                    failures++;
                    if (failures > Constants.HubConnectionMaxRetry)
                    {
                        Logger.LogError($"Device {deviceID} cannot connect to {host} after several attempts.");
                        configuration.Remove(Constants.ConfigHubHost);
                        configuration.Remove(Constants.ConfigHubKey);
                        configuration.Remove(Constants.ConfigDeviceID);
                        configuration.Save();
                        Logger.LogError($"Device provinsioning data purged.");
                    }

                    Thread.Sleep(Constants.HubConnectionRetryInterval);
                    continue;
                }
            } while (AutoConnect);
        }

        public virtual bool ReportTwins()
        {
            bool result = true;
            IConfigurationProvider configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            foreach (string key in configuration.Keys)
            {
                try
                {
                    result &= ReportTwins(new TwinCollection
                        {
                            { key, configuration.GetAsObject(key) }
                        },
                        Constants.DeviceTwinsReportMaxRetry);
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Initial twins reporting has failed.");
                }
            }

            return result;
        }

        protected virtual bool ReportTwins(TwinCollection report, ushort retry)
        {
            if (IsConnected)
            {
                try
                {
                    bool result = deviceClient.UpdateReportedProperties(report);
                    if (result)
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "IoT twins report failed.");
                }
            }

            if (retry <= 0)
            {
                return false;
            }

            Thread.Sleep(Constants.DeviceTwinsReportRetryInterval);
            return ReportTwins(report, --retry);
        }

        protected virtual void UpdateTwins(TwinCollection desired)
        {
            string json = desired.ToJson();
            Hashtable table = (Hashtable)JsonConvert.DeserializeObject(json, typeof(Hashtable));
            IConfigurationProvider configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            bool changed = false;
            foreach (object key in table.Keys)
            {
                string k = (string)key;
                if (k[0] != '$')
                {
                    configuration.Set(k, table[key]);
                    changed = true;
                }
            }

            if (changed)
            {
                configuration.Save();
            }
        }

        protected virtual void FetchIniaialTwins()
        {
            if (IsConnected)
            {
                try
                {
                    Twin twin = deviceClient.GetTwin(new CancellationTokenSource(15000).Token);
                    if (twin != null)
                    {
                        UpdateTwins(twin.Properties.Desired);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Initial twins fetching has failed.");
                }
            }
        }

        protected virtual void DeviceClient_TwinUpdated(object sender, TwinUpdateEventArgs e)
        {
            try
            {
                UpdateTwins(e.Twin);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "Twins Updating has failed.");
            }
        }

        protected virtual string Execute(int requestID, string payload)
        {
            return OnCommandArrived(payload);
        }

        protected virtual bool Open(string assignedIoTHub, string deviceID, string key)
        {
            if (deviceClient != null)
            {
                Close();
            }

            try
            {
                MqttSettings.Instance.ValidateServerCertificate = false;
                deviceClient = new(assignedIoTHub, deviceID, key, qosLevel: MqttQoSLevel.AtLeastOnce, azureCert: new X509Certificate(Constants.AzureRootCerts));
                deviceClient.TwinUpdated += DeviceClient_TwinUpdated;
                deviceClient.AddMethodCallback(Execute);
                bool result = deviceClient.Open();
                if (result)
                {
                    Logger.LogInformation("IoT connection established.");
                }
                else
                {
                    Logger.LogError("IoT connection failed.");
                }

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, "IoT connection fail on open.");
                return false;
            }
            finally
            {
                OnConnectionChanged();
            }
        }

        protected virtual void Close()
        {
            if (deviceClient != null)
            {
                try
                {
                    using (deviceClient)
                    {
                        deviceClient.RemoveMethodCallback(Execute);
                        deviceClient.TwinUpdated -= DeviceClient_TwinUpdated;
                        deviceClient.Close();
                    }
                    deviceClient = null;
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "IoT connection fail on close.");
                }
            }
        }

        protected virtual string OnCommandArrived(string payload)
        {
            return CommandArrived?.Invoke(payload);
        }

        protected virtual void OnConnectionChanged()
        {
            ConnectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AutoConnect = false;
                worker.Join(Constants.NetworkWaitInterval);
                worker = null;
                Close();
            }

            base.Dispose(disposing);
        }
    }
}