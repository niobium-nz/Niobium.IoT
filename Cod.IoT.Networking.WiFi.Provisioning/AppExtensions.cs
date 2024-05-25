using Cod.IoT.Button;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddWiFiProvisioning(this IApp app, int pin, WiFiProvisioningOptions options)
        {
            if (!isLoaded)
            {
                app.AddButton();
                app.RegisterComponent(new WiFiProvissioningCoordinator(pin, options));
                isLoaded = true;
            }

            return app;
        }
    }
}
