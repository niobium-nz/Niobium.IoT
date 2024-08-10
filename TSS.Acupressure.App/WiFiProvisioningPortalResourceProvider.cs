using System;
using System.Text;
using Cod.IoT.Networking.Web.FileSystem;
using Constants = Cod.IoT.Networking.WiFi.Provisioning.Constants;

namespace TSS.Acupressure.App
{
    internal class WiFiProvisioningPortalResourceProvider : IFileBasedWWWResourceProvider
    {
        private const string Index = Constants.WiFiProvinsioningWWWRoot + "\\index.htm";
        private const string Style = Constants.WiFiProvinsioningWWWRoot + "\\style.css";
        private const string Script = Constants.WiFiProvinsioningWWWRoot + "\\script.js";
        private const string KFOmCnqEu92Fr1Mu4mxK = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu4mxK.woff2";
        private const string KFOmCnqEu92Fr1Mu4WxKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu4WxKOzY.woff2";
        private const string KFOmCnqEu92Fr1Mu5mxKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu5mxKOzY.woff2";
        private const string KFOmCnqEu92Fr1Mu72xKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu72xKOzY.woff2";
        private const string KFOmCnqEu92Fr1Mu7GxKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu7GxKOzY.woff2";
        private const string KFOmCnqEu92Fr1Mu7mxKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu7mxKOzY.woff2";
        private const string KFOmCnqEu92Fr1Mu7WxKOzY = Constants.WiFiProvinsioningWWWRoot + "\\KFOmCnqEu92Fr1Mu7WxKOzY.woff2";

        public string[] GetAllResourcePath() => new[] 
        { 
            Index, 
            Style,
            Script, 
            KFOmCnqEu92Fr1Mu4mxK,
            KFOmCnqEu92Fr1Mu4WxKOzY,
            KFOmCnqEu92Fr1Mu5mxKOzY,
            KFOmCnqEu92Fr1Mu72xKOzY,
            KFOmCnqEu92Fr1Mu7GxKOzY,
            KFOmCnqEu92Fr1Mu7mxKOzY,
            KFOmCnqEu92Fr1Mu7WxKOzY
        };

        public byte[] GetResourceContent(string path)
        {
            if (path == Index)
            {
                return Encoding.UTF8.GetBytes(Resources.GetString(Resources.StringResources.index));
            }
            else if (path == Style)
            {
                return Encoding.UTF8.GetBytes(Resources.GetString(Resources.StringResources.style));
            }
            else if (path == Script)
            {
                return Encoding.UTF8.GetBytes(Resources.GetString(Resources.StringResources.script));
            }
            else if (path == KFOmCnqEu92Fr1Mu4mxK)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu4mxK);
            }
            else if (path == KFOmCnqEu92Fr1Mu4WxKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu4WxKOzY);
            }
            else if (path == KFOmCnqEu92Fr1Mu5mxKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu5mxKOzY);
            }
            else if (path == KFOmCnqEu92Fr1Mu72xKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu72xKOzY);
            }
            else if (path == KFOmCnqEu92Fr1Mu7GxKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu7GxKOzY);
            }
            else if (path == KFOmCnqEu92Fr1Mu7mxKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu7mxKOzY);
            }
            else if (path == KFOmCnqEu92Fr1Mu7WxKOzY)
            {
                return Resources.GetBytes(Resources.BinaryResources.KFOmCnqEu92Fr1Mu7WxKOzY);
            }

            throw new NotImplementedException();
        }

        public int GetResourceVersion(string path) => 1;
    }
}
