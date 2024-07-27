namespace Cod.IoT.Networking.WiFi
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent AutoConnectInitiator { get; private set; } = new AutoConnectInitiator();

        public static IApp AddWiFi(this IApp app, bool autoConnect = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.AddCore(enableCommandSupport)
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
