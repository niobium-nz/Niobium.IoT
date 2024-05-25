namespace Cod.IoT.Networking.WiFi
{
    internal class AutoConnectInitiator : GenericComponent, IComponent
    {
        protected override void Initialize()
        {
            INetworkManager networkManager = (INetworkManager)GetService(Constants.NetworkManagerID);
            networkManager.AutoConnect = true;
            networkManager.Connect();
        }
    }
}
