using Microsoft.Extensions.Logging;
using nanoFramework.Networking;
using System;
using System.Net.NetworkInformation;
using System.Threading;

namespace Cod.IoT.Networking.WiFi
{
    internal class WiFiManager : GenericService, INetworkManager
    {
        private Thread worker;

        public bool IsEstablished => WifiNetworkHelper.Status == NetworkHelperStatus.NetworkIsReady;

        public override ushort ID => Constants.NetworkManagerID;

        public bool AutoConnect { get; set; } = false;

        public event EventHandler Established;

        public void Connect()
        {
            if (worker == null)
            {
                worker = new Thread(EnsureConnection);
                worker.Start();
            }
        }

        protected virtual void EnsureConnection()
        {
            do
            {
                if (IsEstablished)
                {
                    Thread.Sleep(Constants.NetworkWaitInterval);
                    continue;
                }

                WifiNetworkHelper.SetupNetworkHelper(requiresDateTime: true);
                NetworkHelper.NetworkReady.WaitOne(Constants.NetworkWaitInterval, true);

                if (!IsEstablished)
                {
                    Logger.LogError($"WiFi connection failed with status {WifiNetworkHelper.Status}.");
                    if (WifiNetworkHelper.HelperException != null)
                    {
                        Logger.LogError($"* Exception: {WifiNetworkHelper.HelperException.Message}\r\nSack = {WifiNetworkHelper.HelperException.StackTrace}");
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
