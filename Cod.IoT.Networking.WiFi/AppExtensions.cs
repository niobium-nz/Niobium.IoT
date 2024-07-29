namespace Cod.IoT.Networking.WiFi
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; } = new AutoConnectInitiator();

        public static IApp UseWiFi(this IApp app, bool autoConnect = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.UseCore(enableCommandSupport)
                   .RegisterService(new WiFiManager());

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
