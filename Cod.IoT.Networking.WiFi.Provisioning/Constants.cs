namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public abstract class Constants : Cod.IoT.Networking.Web.Constants
    {
        public const int WiFiProvinsioningPortalInitializationRetryInterval = 1000;
        public const int WiFiProvinsioningPortalSetupMaxRetry = 3;
        public const int WiFiScanTimeout = 10000;
        public const int WiFiScanCheckInterval = 500;
        public const string WiFiProvinsioningHandlerPath = "/setup";
        public const string WiFiAPScanHandlerPath = "/network-available";
        public const string WiFiProvinsioningWWWRoot = @"I:\provisioning";
        public const string ConfigDevicePIN = "DevicePIN";
        public const string DefaultWiFiProvisioningAccessPointIPAddress = "192.168.177.1";
        public const string DefaultWiFiProvisioningAccessPointNetmask = "255.255.255.0";
        public const string DefaultWiFiProvisioningAccessPointPassword = "88888888";
        public const string WiFiProvinsioningParamSSID = "ssid";
        public const string WiFiProvinsioningParamPassword = "pwd";
        public const string WiFiProvinsioningParamDevicePIN = "pin";
    }
}
