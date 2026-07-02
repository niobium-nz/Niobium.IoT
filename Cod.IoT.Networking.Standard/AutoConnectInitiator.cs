namespace Cod.IoT.Networking
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
