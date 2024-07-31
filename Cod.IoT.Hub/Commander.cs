namespace Cod.IoT.Hub
{
    public class Commander : GenericComponent
    {
        private IHubService hubService;
        private ICommandService commandService;

        protected override void Initialize()
        {
            commandService = (ICommandService)GetService(Constants.CommandServiceID);
            hubService = (IHubService)GetService(Constants.HubServiceID);
            hubService.CommandArrived += HubService_CommandArrived;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                hubService.CommandArrived -= HubService_CommandArrived;
                hubService = null;
                commandService = null;
            }

            base.Dispose(disposing);
        }

        protected virtual DeviceCommandOutput HubService_CommandArrived(string payload)
        {
            return commandService.Execute(payload);
        }
    }
}
