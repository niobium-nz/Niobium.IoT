using Cod.IoT;
using Cod.IoT.Hub.Provisioning;
using Cod.IoT.Networking.WiFi.Provisioning;
using TSS.Acupressure.Motor;

namespace TSS.Acupressure.App
{
    internal class AcupressureApp : ExtendableApp
    {
        private const int MotorSRCLKPin = 5;
        private const int MotorRCLKPin = 19;
        private const int MotorSERPin = 18;
        private const int MotorDriverBitLength = 16;

        private const int MotorSwitchLEDPin = 23;
        private const int MotorSwitchButtonPin = 22;
        private const byte MotorSwitchButtonPressValue = Cod.IoT.Button.Constants.HighPinValue;

        private const int MotorModeLEDPin = 33;
        private const int MotorModeButtonPin = 32;
        private const byte MotorModeButtonPressValue = Cod.IoT.Button.Constants.LowPinValue;

        private const int WiFiProvisioningButtonPin = 32;
        private const byte WiFiProvisioningButtonPressValue = Cod.IoT.Button.Constants.LowPinValue;
        private const int WiFiProvisioningLEDPin = 33;

        private const string WWWRoot = Cod.IoT.Networking.WiFi.Provisioning.Constants.WiFiProvinsioningWWWRoot;
        private const string ProductName = "TAS-Acupressure";
        private const string ServerURL = "https://xxx.com";

        public override void Launch()
        {
            if (!IsInitialized)
            {
                this.AddWiFiProvisioning(WWWRoot, new WiFiProvisioningPortalResourceProvider(), WiFiProvisioningButtonPin, WiFiProvisioningButtonPressValue, new WiFiProvisioningOptions(ProductName), ledPin: WiFiProvisioningLEDPin)
                    .AddDeviceProvisioning(ServerURL)
                    .AddMotor(MotorSRCLKPin, MotorRCLKPin, MotorSERPin, MotorDriverBitLength, MotorSwitchButtonPin, MotorSwitchButtonPressValue, MotorModeButtonPin, MotorModeButtonPressValue, motorSwitchLEDPIN: MotorSwitchLEDPin, motorModeLEDPIN: MotorModeLEDPin);
            }

            base.Launch();
        }
    }
}