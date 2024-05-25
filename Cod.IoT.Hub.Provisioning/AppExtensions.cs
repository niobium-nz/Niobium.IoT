namespace Cod.IoT.Hub.Provisioning
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddDeviceProvisioning(this IApp app, string serverURL, bool autoConnect = true)
        {
            if (!isLoaded)
            {
                app.AddHub(autoConnect);
                app.RegisterComponent(new DeviceProvisioning(serverURL));

                isLoaded = true;
            }

            return app;
        }
    }
}
