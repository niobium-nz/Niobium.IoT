namespace Cod.IoT.Hub.Provisioning
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IComponent DeviceProvisioning { get; private set; }

        public static IApp AddDeviceProvisioning(this IApp app, string serverURL, bool autoConnect = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                DeviceProvisioning ??= new DeviceProvisioning(serverURL);

                app.AddHub(autoConnect: autoConnect, enableCommandSupport: enableCommandSupport)
                   .RegisterComponent(DeviceProvisioning);

                isLoaded = true;
            }

            return app;
        }
    }
}
