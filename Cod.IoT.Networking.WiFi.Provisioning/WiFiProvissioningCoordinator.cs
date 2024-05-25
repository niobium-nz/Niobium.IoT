using Cod.IoT.Button;
using Microsoft.Extensions.Logging;
using nanoFramework.Runtime.Native;
using System.IO;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    internal class WiFiProvissioningCoordinator : GenericComponent
    {
        private readonly int pin;
        private readonly WiFiProvisioningOptions options;
        private IButtonService buttonService;
        private WebServer server;

        public WiFiProvissioningCoordinator(int pin, WiFiProvisioningOptions options)
        {
            this.pin = pin;
            this.options = options;
        }

        protected override void Initialize()
        {
            buttonService = (IButtonService)GetService(Constants.ButtonServiceID);
            buttonService.RegisterInterest(pin, true);
            buttonService.Held += ButtonService_Held;

            if (File.Exists(Constants.WiFiProvinsioningRequestedFile))
            {
                File.Delete(Constants.WiFiProvinsioningRequestedFile);
                SetupAP();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                buttonService.UnregisterInterest(pin);
                buttonService.Held -= ButtonService_Held;
                buttonService = null;

                server?.Stop();
                server = null;
            }

            base.Dispose(disposing);
        }

        private void ButtonService_Held(int pin)
        {
            if (pin == this.pin)
            {
                SetupAP();
            }
        }

        private void SetupAP()
        {
            bool success = WirelessAP.SetWifiAp(options.IPAddress, options.BoardcastSSID);
            if (success)
            {
                server ??= new WebServer(options);
                server.Start();
            }
            else if (!File.Exists(Constants.WiFiProvinsioningRequestedFile))
            {
                // Reboot device to Activate Access Point on restart
                File.Create(Constants.WiFiProvinsioningRequestedFile);
                Logger.LogInformation($"WiFi provissioning access point setup completed, rebooting...");
                Power.RebootDevice();
            }
        }
    }
}
