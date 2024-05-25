namespace Cod.IoT.Hub
{
    internal class AutoConnectInitiator : GenericComponent, IComponent
    {
        protected override void Initialize()
        {
            IHubService hubService = (IHubService)GetService(Constants.HubServiceID);
            hubService.AutoConnect = true;
            hubService.Connect();
        }
    }
}
