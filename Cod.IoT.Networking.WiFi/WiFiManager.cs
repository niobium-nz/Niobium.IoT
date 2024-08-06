using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Threading;
using Microsoft.Extensions.Logging;
using nanoFramework.Networking;

namespace Cod.IoT.Networking.WiFi
{
    public class WiFiManager : GenericService, INetworkManager
    {
        protected Thread worker;

        public virtual bool IsEstablished => WifiNetworkHelper.Status == NetworkHelperStatus.NetworkIsReady;

        public override int ID => Constants.NetworkManagerID;

        public bool AutoConnect { get; set; } = false;

        public event EventHandler Established;

        protected virtual bool IsReady
        {
            get
            {
                Wireless80211Configuration wconf = Helper.GetWiFiConfiguration();
                return wconf != null && !string.IsNullOrEmpty(wconf.Ssid);
            }
        }

        public byte[] PhysicalAddress => Helper.GetWiFiInterface().PhysicalAddress;

        public virtual void Connect()
        {
            if (worker == null)
            {
                if (!File.Exists(Constants.NetworkProvinsioningRequestedFile))
                {
                    worker = new Thread(EnsureConnection);
                    worker.Start();
                }
                else
                {
                    Logger.LogInformation($"WiFi not automatically connected due to its provinsioning status");
                }
            }
        }

        protected virtual void EnsureConnection()
        {
            do
            {
                if (IsEstablished || !IsReady)
                {
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                if (!WifiNetworkHelper.Reconnect(true, token: new CancellationTokenSource(Constants.NetworkWaitInterval).Token))
                {
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Logger.LogError(WifiNetworkHelper.HelperException, $"WiFi connection failed with status {WifiNetworkHelper.Status}.");
                    }
                    continue;
                }

                OnEstablished();
            } while (AutoConnect);
        }

        protected virtual void OnEstablished()
        {
            Logger.LogInformation($"WiFi connection successfully established with IP: {IPGlobalProperties.GetIPAddress()}.");
            Established?.Invoke(this, EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                AutoConnect = false;
                worker = null;
            }

            base.Dispose(disposing);
        }
    }
}
