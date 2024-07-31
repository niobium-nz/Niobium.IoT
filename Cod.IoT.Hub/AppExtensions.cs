namespace Cod.IoT.Hub
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; } = new AutoConnectInitiator();
        public static IComponent Commander { get; private set; } = new Commander();
        public static IAction RebootAction { get; private set; } = new RebootAction();
        public static IAction DownloadAction { get; private set; } = new DownloadAction();
        public static IService HubService { get; private set; } = new HubService();

        public static IApp UseHub(this IApp app, bool autoConnect, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.UseCore(enableCommandSupport)
                   .RegisterService(HubService);

                if (autoConnect)
                {
                    app.RegisterComponent(AutoConnectInitiator)
                       .RegisterComponent(Commander);
                }

                if (Cod.IoT.AppExtensions.IsCommandSupportEnabled)
                {
                    app.RegisterAction(RebootAction)
                       .RegisterAction(DownloadAction);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
