using Cod.IoT.Networking;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Cod.IoT.Hub.Provisioning
{
    internal class DeviceProvisioning : GenericComponent
    {
        private bool isStopRequested;
        private readonly string serverURL;

        public DeviceProvisioning(string serverURL)
        {
            this.serverURL = serverURL;
        }

        protected override void Initialize()
        {
            isStopRequested = false;
            new Thread(ProvisionDevice).Start();
        }

        protected virtual void ProvisionDevice()
        {
            IConfigurationProvider configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            while (!isStopRequested)
            {
                string host = configuration.GetAsString(Constants.ConfigHubHost);
                string key = configuration.GetAsString(Constants.ConfigHubKey);
                string deviceID = configuration.GetAsString(Constants.ConfigDeviceID);
                if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(deviceID))
                {
                    Logger.LogInformation($"Device {deviceID} has previously been provisioned to {host}.");
                    return;
                }

                string pin = configuration.GetAsString(Constants.ConfigDevicePIN);
                if (string.IsNullOrEmpty(pin))
                {
                    Logger.LogError($"Device provisioning failed due to invalid PIN.");
                    return;
                }

                INetworkManager networkManager = (INetworkManager)GetService(Constants.NetworkManagerID);
                if (!networkManager.IsEstablished)
                {
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                Logger.LogInformation($"Provisioning deivce {deviceID} against {serverURL}...");

                try
                {
                    configuration.Set(Constants.ConfigDeviceID, "test123");
                    configuration.Set(Constants.ConfigHubHost, "testtashub1.azure-devices.net");
                    configuration.Set(Constants.ConfigHubKey, "5n2i4cJBp/a0lIpT4tpwMy9TGqqAlHoy7AIoTF3IG8U=");
                    configuration.Save();
                    return;
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, $"Device registration failed.");
                    Thread.Sleep(Constants.HubConnectionRetryInterval);
                    continue;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                isStopRequested = true;
            }

            base.Dispose(disposing);
        }
    }
}