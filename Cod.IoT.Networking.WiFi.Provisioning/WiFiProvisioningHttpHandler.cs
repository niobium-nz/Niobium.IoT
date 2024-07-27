using System.IO;
using System.Net;
using System.Threading;
using Cod.IoT.Networking.Web;
using Microsoft.Extensions.Logging;
using nanoFramework.Runtime.Native;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    public class WiFiProvisioningHttpHandler : GenericHttpHandler
    {
        protected override string Method => Constants.HttpPostMethod;
        protected override bool IsSupported(string Path) => Path == Constants.WiFiProvinsioningHandlerPath;

        protected override void Handle(HttpListenerRequest request, HttpListenerResponse response)
        {
            var param = request.GetFormParams();
            if (!param.Contains(Constants.WiFiProvinsioningParamSSID) || !param.Contains(Constants.WiFiProvinsioningParamPassword) || !param.Contains(Constants.WiFiProvinsioningParamDevicePIN))
            {
                response.SendResponse("Required request parameters must be provided.", HttpStatusCode.BadRequest);
                return;
            }

            var ssid = param[Constants.WiFiProvinsioningParamSSID] as string;
            var pwd = param[Constants.WiFiProvinsioningParamPassword] as string;
            var pin = param[Constants.WiFiProvinsioningParamDevicePIN] as string;
            if (string.IsNullOrEmpty(ssid) || string.IsNullOrEmpty(pwd) || string.IsNullOrEmpty(pin))
            {
                response.SendResponse("Invalid request parameter detected.", HttpStatusCode.BadRequest);
                return;
            }

            Logger.LogInformation($"Provisioning device: {Constants.WiFiProvinsioningParamSSID}={ssid}, {Constants.WiFiProvinsioningParamPassword}={pwd}, {Constants.WiFiProvinsioningParamDevicePIN}={pin}");
            bool result = Wireless80211.Configure(ssid, pwd);
            if (!result)
            {
                response.SendResponse($"Unable to setup WiFi connection to {ssid}.", HttpStatusCode.BadRequest);
                return;
            }

            IConfigurationProvider configuration = (IConfigurationProvider)GetService(Constants.ConfigurationProviderID);
            configuration.Set(Constants.ConfigDevicePIN, pin);
            configuration.Save();
            response.StatusCode = (int)HttpStatusCode.OK;
        }

        public override void PostHandle()
        {
            WirelessAP.Disable();
            try
            {
                if (File.Exists(Constants.NetworkProvinsioningRequestedFile))
                {
                    File.Delete(Constants.NetworkProvinsioningRequestedFile);
                }
            }
            catch
            {
            }

            Thread.Sleep(200);
            Logger.LogInformation("WiFi connection has been setup, rebooting...");
            Power.RebootDevice();
        }
    }
}
