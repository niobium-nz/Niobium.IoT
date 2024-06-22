using System.Collections;
using System.Device.Wifi;
using System.Net.NetworkInformation;
using System.Threading;
using nanoFramework.Networking;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    internal class Wireless80211
    {
        private static bool scanInProgress = false;
        private static ArrayList wifiScanResult;

        /// <summary>
        /// Disable the Wireless station interface.
        /// </summary>
        public static void Disable()
        {
            Wireless80211Configuration wconf = Helper.GetWiFiConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.None | Wireless80211Configuration.ConfigurationOptions.SmartConfig;
            wconf.SaveConfiguration();
        }

        /// <summary>
        /// Configure and enable the Wireless station interface
        /// </summary>
        /// <param name="ssid"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Configure(string ssid, string password)
        {
            // Make sure we are disconnected before we start connecting otherwise
            // ConnectDhcp will just return success instead of reconnecting.
            WifiAdapter wa = WifiAdapter.FindAllAdapters()[0];
            wa.Disconnect();

            CancellationTokenSource cs = new(30_000);
            WifiNetworkHelper.Disconnect();

            // Reconfigure properly the normal wifi
            Wireless80211Configuration wconf = Helper.GetWiFiConfiguration();
            wconf.Options = Wireless80211Configuration.ConfigurationOptions.AutoConnect | Wireless80211Configuration.ConfigurationOptions.Enable;
            wconf.Ssid = ssid;
            wconf.Password = password;
            wconf.SaveConfiguration();

            WifiNetworkHelper.Disconnect();
            bool success;

            success = WifiNetworkHelper.ConnectDhcp(ssid, password, WifiReconnectionKind.Automatic, true, token: cs.Token);

            if (!success)
            {
                wa.Disconnect();
                // Bug in network helper, we've most likely try to connect before, let's make it manual
                WifiConnectionResult res = wa.Connect(ssid, WifiReconnectionKind.Automatic, password);
                success = res.ConnectionStatus == WifiConnectionStatus.Success;
            }

            return success;
        }

        public static WiFiNetwork[] Scan()
        {
            wifiScanResult ??= new();
            wifiScanResult.Clear();
            WifiAdapter adapter = WifiAdapter.FindAllAdapters()[0];
            adapter.AvailableNetworksChanged += Wifi_AvailableNetworksChanged;
            adapter.ScanAsync();
            scanInProgress = true;
            var mslapsed = 0;
            while (scanInProgress)
            {
                Thread.Sleep(Constants.WiFiScanCheckInterval);
                mslapsed += Constants.WiFiScanCheckInterval;
                if (mslapsed > Constants.WiFiScanTimeout)
                {
                    break;
                }
            }
            var result = new WiFiNetwork[wifiScanResult.Count];
            wifiScanResult.CopyTo(result);
            wifiScanResult.Clear();
            return result;
        }

        private static void Wifi_AvailableNetworksChanged(WifiAdapter sender, object e)
        {
            try
            {
                WifiNetworkReport report = sender.NetworkReport;
                foreach (WifiAvailableNetwork net in report.AvailableNetworks)
                {
                    wifiScanResult.Add(new WiFiNetwork
                    {
                        SSID = net.Ssid,
                        RSSI = net.NetworkRssiInDecibelMilliwatts,
                        BSSID = net.Bsid,
                    });
                }
            }
            catch
            {
            }
            finally
            {
                scanInProgress = false;
            }
        }
    }
}
