using System;
using Cod.IoT.Networking.Web.FileSystem;
using Constants = Cod.IoT.Networking.WiFi.Provisioning.Constants;

namespace TSS.Acupressure.App
{
    internal class WiFiProvisioningPortalResourceProvider : IFileBasedWWWResourceProvider
    {
        private const string Index = Constants.WiFiProvinsioningWWWRoot + "\\index.htm";
        private const string Style = Constants.WiFiProvinsioningWWWRoot + "\\style.css";
        private const string Script = Constants.WiFiProvinsioningWWWRoot + "\\script.js";

        public string[] GetAllResourcePath() => new[] { Index, Style, Script };

        public string GetResourceContent(string path)
        {
            if (path == Index)
            {
                return Resources.GetString(Resources.StringResources.index);
            }
            else if (path == Style)
            {
                return Resources.GetString(Resources.StringResources.style);
            }
            else if (path == Script)
            {
                return Resources.GetString(Resources.StringResources.script);
            }

            throw new NotImplementedException();
        }
    }
}
