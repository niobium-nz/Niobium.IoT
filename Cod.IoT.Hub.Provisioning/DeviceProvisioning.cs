using Cod.IoT.Networking;
using Cod.IoT.Networking.Web;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace Cod.IoT.Hub.Provisioning
{
    public class DeviceProvisioning : GenericComponent
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
                string key = configuration.GetAsString(Constants.ConfigHubPrimaryKey);
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


                try
                {
                    var mac = BitConverter.ToString(networkManager.PhysicalAddress);
                    Logger.LogInformation($"Provisioning deivce {mac} against {serverURL}...");
                    var response = HTTP.Post(serverURL, new DeviceProvisioningRequest
                    {
                        PIN = pin,
                        UID = mac,
                    });

                    if (response == null)
                    {
                        Logger.LogError($"Device provisioning failed without response.");
                        Thread.Sleep(Constants.NetworkWaitInterval);
                        continue;
                    }

                    if (response.Status < 200 || response.Status >= 400)
                    {
                        Logger.LogError($"Device provisioning failed with status {response.Status}: {response.Body}.");
                        Thread.Sleep(Constants.NetworkWaitInterval);
                        continue;
                    }

                    if (response != null)
                    {
                        var result = (DeviceProvisioningResponse)JSON.Instance.Deserialize(response.Body, typeof(DeviceProvisioningResponse));
                        if (result == null 
                            || string.IsNullOrEmpty(result.DeviceID)
                            || string.IsNullOrEmpty(result.PrimaryKey)
                            || string.IsNullOrEmpty(result.SecondaryKey)
                            || string.IsNullOrEmpty(result.AssignedHub))
                        {
                            Logger.LogError($"Device provisioning failed with invalid result: {response.Status}: {response.Body}.");
                            Thread.Sleep(Constants.NetworkWaitInterval);
                            continue;
                        }

                        configuration.Set(Constants.ConfigDeviceID, result.DeviceID);
                        configuration.Set(Constants.ConfigHubHost, result.AssignedHub);
                        configuration.Set(Constants.ConfigHubPrimaryKey, result.PrimaryKey);

                        if (!string.IsNullOrEmpty(result.SecondaryKey))
                        {
                            configuration.Set(Constants.ConfigHubSecondaryKey, result.SecondaryKey);
                        }

                        configuration.Save();
                        Logger.LogInformation($"Provisioning deivce {result.DeviceID} successfully assigned to {result.AssignedHub}.");
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, "Device registration failed.");
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