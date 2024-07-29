using Cod.IoT.Button;
using Cod.IoT.Indicator;
using Cod.IoT.Networking.Web;
using Cod.IoT.Networking.Web.FileSystem;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IHttpHandler WiFiProvisioningHttpHandler { get; private set; } = new WiFiProvisioningHttpHandler();
        public static IHttpHandler WiFiAPScanHttpHandler { get; private set; } = new WiFiAPScanHttpHandler();
        public static IComponent WiFiProvisioningCoordinator { get; private set; }

        public static IApp UseWiFiProvisioning(this IApp app,
            string wwwroot, IFileBasedWWWResourceProvider wwwresourceprovider,
            int buttonPin, byte pressPinValue, WiFiProvisioningOptions options, int ledPin = -1,
            bool autoStartReadingGPIO = true, bool autoConnect = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                WiFiProvisioningCoordinator ??= new WiFiProvisioningCoordinator(buttonPin, pressPinValue, options, ledPin);

                app.UseWiFi(autoConnect: autoConnect, enableCommandSupport: enableCommandSupport)
                   .UseButton(autoStartReadingGPIO: autoStartReadingGPIO, enableCommandSupport: enableCommandSupport)
                   .UseIndicator(enableCommandSupport)
                   .UseWeb(wwwroot, wwwresourceprovider, new[] { WiFiAPScanHttpHandler, WiFiProvisioningHttpHandler })
                   .RegisterComponent(WiFiProvisioningCoordinator);

                isLoaded = true;
            }

            return app;
        }
    }
}
