using System.Net.NetworkInformation;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public abstract class Helper : Cod.IoT.Networking.WiFi.Helper
    {
        public static WirelessAPConfiguration GetWiFiAPConfiguration()
        {
            NetworkInterface ni = GetWiFiAPInterface();
            if (ni == null)
            {
                return null;
            }

            var configurations = WirelessAPConfiguration.GetAllWirelessAPConfigurations();
            if (configurations.Length > ni.SpecificConfigId)
            {
                return configurations[ni.SpecificConfigId];
            }

            return null;
        }

        public static NetworkInterface GetWiFiAPInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.WirelessAP)
                {
                    return ni;
                }
            }
            return null;
        }

        public static string GetWiFiAPIP()
        {
            NetworkInterface ni = GetWiFiAPInterface();
            return ni.IPv4Address;
        }
    }
}
