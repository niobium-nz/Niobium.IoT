namespace Cod.IoT.Networking.WiFi
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; }
        public static INetworkManager WiFiManager { get; private set; }

        public static IApp UseWiFi(this IApp app, bool autoConnect = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                AutoConnectInitiator ??= new AutoConnectInitiator();
                WiFiManager ??= new WiFiManager();

                app.UseCore(enableCommandSupport)
                   .RegisterService(WiFiManager);

                if (autoConnect)
                {
                    app.RegisterComponent(AutoConnectInitiator);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
