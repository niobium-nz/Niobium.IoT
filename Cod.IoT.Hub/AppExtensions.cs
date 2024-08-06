namespace Cod.IoT.Hub
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; }
        public static IComponent Commander { get; private set; }
        public static IAction RebootAction { get; private set; }
        public static IAction DownloadAction { get; private set; }
        public static IService HubService { get; private set; }

        public static IApp UseHub(this IApp app, bool autoConnect, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                AutoConnectInitiator = new AutoConnectInitiator();
                Commander = new Commander();
                RebootAction = new RebootAction();
                DownloadAction = new DownloadAction();
                HubService = new HubService();

                app.UseCore(enableCommandSupport)
                    .RegisterService(HubService)
                    .RegisterAction(RebootAction)
                    .RegisterAction(DownloadAction);

                if (autoConnect)
                {
                    app.RegisterComponent(AutoConnectInitiator)
                       .RegisterComponent(Commander);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
