namespace Cod.IoT.Networking.WiFi
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddWiFi(this IApp app, bool autoConnect = true)
        {
            if (!isLoaded)
            {
                app.AddCore();
                app.RegisterService(Constants.NetworkManagerID, new WiFiManager());
                if (autoConnect)
                {
                    app.RegisterComponent(new AutoConnectInitiator());
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
