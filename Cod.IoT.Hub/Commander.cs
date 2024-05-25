namespace Cod.IoT.Hub
{
    internal class Commander : GenericComponent
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

        private string HubService_CommandArrived(string payload)
        {
            return (string)commandService.Execute(payload);
        }
    }
}
