using System.Net;
using Cod.IoT.Networking.Web;
using Microsoft.Extensions.Logging;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public class WiFiAPScanHttpHandler : GenericHttpHandler
    {
        protected override string Method => Constants.HttpGetMethod;

        protected override bool IsSupported(string Path) => Path == Constants.WiFiAPScanHandlerPath;

        protected override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            Logger.LogInformation("Scanning for WiFi AP available...");
            var result = Wireless80211.Scan();
            response.SendJsonResponse(result);
        }
    }
}
