namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public class WiFiProvisioningOptions
    {
        public string BoardcastSSID { get; private set; }

        public string Password { get; private set; }

        public string IPAddress { get; private set; }

        public string Netmask { get; private set; }

        public WiFiProvisioningOptions(string ssid)
            : this(ssid, Constants.DefaultWiFiProvisioningAccessPointPassword, Constants.DefaultWiFiProvisioningAccessPointIPAddress, Constants.DefaultWiFiProvisioningAccessPointNetmask)
        {

        }

        public WiFiProvisioningOptions(string ssid, string password, string ip, string netmask)
        {
            BoardcastSSID = ssid;
            Password = password;
            IPAddress = ip;
            Netmask = netmask;
        }
    }
}
