using System;
using System.Threading;
using Cod.IoT;
using Cod.IoT.Hub;
using Cod.IoT.Hub.Provisioning;
using Cod.IoT.Indicator;
using Cod.IoT.Networking.WiFi.Provisioning;
using TSS.Acupressure.Motor;

namespace TSS.Acupressure.App
{
    internal class AcupressureApp : ExtendableApp
    {
        private const int MotorSRCLKPin = 5;
        private const int MotorRCLKPin = 19;
        private const int MotorSERPin = 22;
        private const int MotorDriverBitLength = 16;

        private const int MotorSwitchLEDPin = 33;
        private const int MotorSwitchButtonPin = 32;
        private const byte MotorSwitchButtonPressValue = Cod.IoT.Button.Constants.LowPinValue;

        private const int WiFiProvisioningButtonPin = 32;
        private const byte WiFiProvisioningButtonPressValue = Cod.IoT.Button.Constants.LowPinValue;
        private const int WiFiProvisioningLEDPin = 33;

        private const string WWWRoot = Cod.IoT.Networking.WiFi.Provisioning.Constants.WiFiProvinsioningWWWRoot;
        private const string ProductName = "TAS-Acupressure";
        private const string ServerURL = "https://xxx.com";

        private const int MotorReadyIndicationTimes = 2;
        private const int HubReadyIndicationTimes = 3;

        private IHubService hubService;

        public override void Launch()
        {
            if (!IsInitialized)
            {
                this.AddWiFiProvisioning(WWWRoot, new WiFiProvisioningPortalResourceProvider(), WiFiProvisioningButtonPin, WiFiProvisioningButtonPressValue, new WiFiProvisioningOptions(ProductName), ledPin: WiFiProvisioningLEDPin)
                    .AddDeviceProvisioning(ServerURL)
                    .AddMotor(MotorSRCLKPin, MotorRCLKPin, MotorSERPin, MotorDriverBitLength, MotorSwitchButtonPin, MotorSwitchButtonPressValue, motorSwitchLEDPIN: MotorSwitchLEDPin);
            }

            base.Launch();

            hubService = (IHubService)GetService(Cod.IoT.Hub.Constants.HubServiceID);
            if (hubService.IsConnected)
            {
                Indicate(HubReadyIndicationTimes);
            }
            else
            {
                hubService.ConnectionChanged += HubService_ConnectionChanged;
                Indicate(MotorReadyIndicationTimes);
            }
        }

        protected virtual void HubService_ConnectionChanged(object sender, EventArgs e)
        {
            if (sender is IHubService hubService && hubService.IsConnected)
            {
                hubService.ConnectionChanged -= HubService_ConnectionChanged;
                Indicate(HubReadyIndicationTimes);
            }
        }

        protected virtual void Indicate(int times)
        {
            var indicatorService = (IIndicatorService)GetService(Cod.IoT.Indicator.Constants.IndicatorServiceID);
            for (int i = 0; i < times; i++)
            {
                indicatorService.TurnOn(MotorSwitchLEDPin);
                Thread.Sleep(300);
                indicatorService.TurnOff(MotorSwitchLEDPin);
                Thread.Sleep(300);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (hubService != null)
                { 
                    hubService.ConnectionChanged -= HubService_ConnectionChanged;
                }
                hubService = null;
            }

            base.Dispose(disposing);
        }
    }
}