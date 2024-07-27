using System.Net.NetworkInformation;

namespace Cod.IoT.Networking.WiFi
{
    public abstract class Helper : Cod.IoT.Helper
    {
        public static Wireless80211Configuration GetWiFiConfiguration()
        {
            NetworkInterface ni = GetWiFiInterface();
            if (ni == null)
            {
                return null;
            }

            var configurations = Wireless80211Configuration.GetAllWireless80211Configurations();
            if (configurations.Length > ni.SpecificConfigId)
            {
                return configurations[ni.SpecificConfigId];
            }

            return null;
        }

        public static NetworkInterface GetWiFiInterface()
        {
            NetworkInterface[] Interfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface ni in Interfaces)
            {
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                {
                    return ni;
                }
            }
            return null;
        }

        public static string GetWiFiIP()
        {
            NetworkInterface ni = GetWiFiInterface();
            return ni.IPv4Address;
        }
    }
}
