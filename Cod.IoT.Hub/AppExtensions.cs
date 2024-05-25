namespace Cod.IoT.Hub
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddHub(this IApp app, bool autoConnect)
        {
            if (!isLoaded)
            {
                app.AddCore();
                app.RegisterService(Constants.HubServiceID, new HubService());
                if (autoConnect)
                {
                    app.RegisterComponent(new AutoConnectInitiator());
                    app.RegisterComponent(new Commander());
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
