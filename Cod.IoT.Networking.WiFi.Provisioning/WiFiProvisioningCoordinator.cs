using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using Cod.IoT.Button;
using Cod.IoT.Indicator;
using Cod.IoT.Networking.Web;
using Iot.Device.DhcpServer;
using Microsoft.Extensions.Logging;
using nanoFramework.Runtime.Native;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public class WiFiProvisioningCoordinator : GenericComponent
    {
        private readonly int buttonPin;
        private readonly int ledPin;
        private readonly byte pressPinValue;
        private readonly WiFiProvisioningOptions options;
        private IButtonService buttonService;
        private ITaskService taskService;
        private IIndicatorService indicatorService;
        private IWebServer server;

        public WiFiProvisioningCoordinator(int buttonPin, byte pressPinValue, WiFiProvisioningOptions options, int ledPin = -1)
        {
            this.buttonPin = buttonPin;
            this.ledPin = ledPin;
            this.pressPinValue = pressPinValue;
            this.options = options;
        }

        protected override void Initialize()
        {
            server = (IWebServer)GetService(Constants.WebServerID);
            buttonService = (IButtonService)GetService(Button.Constants.ButtonServiceID);
            indicatorService = (IIndicatorService)GetService(Indicator.Constants.IndicatorServiceID);
            taskService = (ITaskService)GetService(Constants.TaskServiceID);

            var remainingRetry = GetProvisioningRemainingRetry();
            if (remainingRetry > 0)
            {
                Logger.LogInformation($"Starting WiFi provisioning process, remaining retry: {remainingRetry}");
                remainingRetry--;
                SetProvisioningRemainingRetry(remainingRetry);
                StartProvisioning();
            }
            else
            {
                buttonService.RegisterPress(buttonPin, true, pressPinValue);
                buttonService.Held += ButtonService_Held;
            }
        }

        protected virtual void StartProvisioning()
        {
            if (ledPin > 0)
            {
                indicatorService.StartBlink(ledPin);
            }

            taskService.Schedule(SetupProvisioningPortal);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (buttonService != null)
                {
                    buttonService.Unregister(buttonPin);
                    buttonService.Held -= ButtonService_Held;
                }
                buttonService = null;
                taskService = null;
                indicatorService = null;

                server?.Stop();
                server = null;
            }

            base.Dispose(disposing);
        }

        protected virtual void ButtonService_Held(int pin)
        {
            if (pin == this.buttonPin)
            {
                SetupAP();
            }
        }

        protected virtual void SetupProvisioningPortal()
        {
            Thread.Sleep(Constants.WiFiProvinsioningPortalInitializationRetryInterval);

            NetworkInterface ni = Helper.GetWiFiAPInterface();
            if (ni.IPv4Address != options.IPAddress)
            {
                Logger.LogWarning($"Current IP is not expected: {ni.IPv4Address}");
                ni.EnableStaticIPv4(options.IPAddress, options.Netmask, options.IPAddress);
                Logger.LogInformation("Static IP updated, rebooting...");
                Power.RebootDevice();
                return;
            }

            Thread.Sleep(3000);
            var result = StartDHCPServer();
            if (result)
            {
                result = StartWebServer();
            }

            if (!result)
            {
                Logger.LogError("Failed to setup WiFi provisioning portal, rebooting...");
                Power.RebootDevice();
            }
        }

        protected virtual bool StartWebServer()
        {
            var result = server.Start(80, options.IPAddress);
            Logger.LogInformation($"WiFi provisioning portal WEB server started at {options.IPAddress}:80");
            return result;
        }

        protected virtual bool StartDHCPServer()
        {
            var result = false;
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    DhcpServer dhcpserver = new()
                    {
                        CaptivePortalUrl = $"http://{options.IPAddress}/"
                    };

                    result |= dhcpserver.Start(IPAddress.Parse(options.IPAddress), IPAddress.Parse(options.Netmask));
                    Logger.LogInformation($"WiFi provisioning portal DHCP server started: {result}");
                    if (result)
                    {
                        break;
                    }
                }
                catch (Exception e)
                {
                    Logger.LogCritical(e, "Failed to start WiFi provisioning DHCP server.");
                }
            }

            return result;
        }

        protected virtual void SetupAP()
        {
            Wireless80211.Disable();
            var setupResult = WirelessAP.Setup(options.IPAddress, options.Netmask, options.BoardcastSSID, options.Password);
            if (!setupResult)
            {
                Logger.LogInformation("Configured WiFi provisioning portal, reboot pending...");
            }

            SetProvisioningRemainingRetry(Constants.WiFiProvinsioningPortalSetupMaxRetry);
            if (ledPin > 0)
            {
                indicatorService.StartBlink(ledPin);
            }
        }

        protected static int GetProvisioningRemainingRetry()
        {
            if (File.Exists(Constants.NetworkProvinsioningRequestedFile))
            {
                try
                {
                    var remainingRetry = File.ReadAllText(Constants.NetworkProvinsioningRequestedFile);
                    if (!int.TryParse(remainingRetry, out var currentRetry))
                    {
                        return 1;
                    }
                    return currentRetry;
                }
                catch (Exception)
                {
                    return 1;
                }
            }
            else
            {
                return 0;
            }
        }

        protected static void SetProvisioningRemainingRetry(int value)
        {
            try
            {
                if (value <= 0)
                {
                    if (File.Exists(Constants.NetworkProvinsioningRequestedFile))
                    {
                        File.Delete(Constants.NetworkProvinsioningRequestedFile);
                    }
                }
                else
                {
                    File.WriteAllText(Constants.NetworkProvinsioningRequestedFile, value.ToString());
                }
            }
            catch
            {
            }
        }
    }
}
