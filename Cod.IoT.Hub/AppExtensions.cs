namespace Cod.IoT.Hub
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; } = new AutoConnectInitiator();
        public static IComponent Commander { get; private set; } = new Commander();
        public static ICommand RebootCommand { get; private set; } = new RebootCommand();
        public static ICommand DownloadCommand { get; private set; } = new DownloadCommand();

        public static IApp UseHub(this IApp app, bool autoConnect, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.UseCore(enableCommandSupport)
                   .RegisterService(new HubService());

                if (autoConnect)
                {
                    app.RegisterComponent(AutoConnectInitiator)
                       .RegisterComponent(Commander);
                }

                if (Cod.IoT.AppExtensions.IsCommandSupportEnabled)
                {
                    app.RegisterCommand(RebootCommand)
                       .RegisterCommand(DownloadCommand);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
