using nanoFramework.Runtime.Native;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;

namespace Cod.IoT.Networking.WiFi.Provisioning
{
    internal class WebServer
    {
        private HttpListener listener;
        private Thread serverThread;
        private readonly WiFiProvisioningOptions options;

        public WebServer(WiFiProvisioningOptions options)
        {
            this.options = options;
        }

        public void Start()
        {
            if (listener == null)
            {
                listener = new HttpListener("http");
                serverThread = new Thread(RunServer);
                serverThread.Start();
            }
        }

        public void Stop()
        {
            listener?.Stop();
        }

        private void RunServer()
        {
            listener.Start();

            while (listener.IsListening)
            {
                HttpListenerContext context = listener.GetContext();
                if (context != null)
                {
                    ProcessRequest(context);
                }
                else
                {
                    Thread.Sleep(100);
                }
            }

            listener.Close();
            listener = null;
        }

        private void ProcessRequest(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;
            HttpListenerResponse response = context.Response;
            string responseString;
            string ssid = null;
            string password = null;
            bool isApSet = false;

            switch (request.HttpMethod)
            {
                case "GET":
                    string[] url = request.RawUrl.Split('?');
                    if (url[0] == "/favicon.ico")
                    {
                        response.ContentType = "image/png";
                        SendByteResponse(response, options.Favicon);
                    }
                    else
                    {
                        response.ContentType = "text/html";
                        responseString = ReplaceMessage(options.IndexHTML, string.Empty);
                        SendResponse(response, responseString);
                    }
                    break;

                case "POST":
                    // Pick up POST parameters from Input Stream
                    Hashtable hashPars = ParseParamsFromStream(request.InputStream);
                    ssid = (string)hashPars["ssid"];
                    password = (string)hashPars["password"];

                    Debug.WriteLine($"Wireless parameters SSID:{ssid} PASSWORD:{password}");

                    string message = "<p>New settings saved.</p><p>Rebooting device to put into normal mode</p>";

                    bool res = Wireless80211.Configure(ssid, password);
                    if (res)
                    {
                        message += $"<p>And your new IP address should be {Wireless80211.GetCurrentIPAddress()}.</p>";
                    }

                    responseString = CreateMainPage(message);

                    SendResponse(response, responseString);
                    isApSet = true;
                    break;
            }

            response.Close();

            if (isApSet && (!string.IsNullOrEmpty(ssid)) && (!string.IsNullOrEmpty(password)))
            {
                // Enable the Wireless station interface
                Wireless80211.Configure(ssid, password);

                // Disable the Soft AP
                WirelessAP.Disable();
                Thread.Sleep(200);
                Power.RebootDevice();
            }
        }

        private static string ReplaceMessage(string html, string message)
        {
            int index = html.IndexOf(Constants.HTMLContentPlaceholder);
            return index >= 0 ? html.Substring(0, index) + message + html.Substring(index + 9) : html;
        }

        private static void SendResponse(HttpListenerResponse response, string responseString)
        {
            SendByteResponse(response, System.Text.Encoding.UTF8.GetBytes(responseString));
        }

        private static void SendByteResponse(HttpListenerResponse response, byte[] responseBytes)
        {
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }

        private static Hashtable ParseParamsFromStream(Stream inputStream)
        {
            byte[] buffer = new byte[inputStream.Length];
            inputStream.Read(buffer, 0, (int)inputStream.Length);

            return ParseParams(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
        }

        private static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
                hash.Add(nameValue[0], nameValue[1]);
            }

            return hash;
        }

        private static string CreateMainPage(string message)
        {

            return $"<!DOCTYPE html><html>{GetCss()}<body>" +
                    "<h1>NanoFramework</h1>" +
                    "<form method='POST'>" +
                    "<fieldset><legend>Wireless configuration</legend>" +
                    "Ssid:</br><input type='input' name='ssid' value='' ></br>" +
                    "Password:</br><input type='password' name='password' value='' >" +
                    "<br><br>" +
                    "<input type='submit' value='Save'>" +
                    "</fieldset>" +
                    "<b>" + message + "</b>" +
                    "</form></body></html>";
        }

        private static string GetCss()
        {
            return "<head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><style>" +
                "*{box-sizing: border-box}" +
                "h1,legend {text-align:center;}" +
                "form {max-width: 250px;margin: 10px auto 0 auto;}" +
                "fieldset {border-radius: 5px;box-shadow: 3px 3px 15px hsl(0, 0%, 90%);font-size: large;}" +
                "input {width: 100%;padding: 4px;margin-bottom: 8px;border: 1px solid hsl(0, 0%, 50%);border-radius: 3px;font-size: medium;}" +
                "input[type=submit]:hover {cursor: pointer;background-color: hsl(0, 0%, 90%);transition: 0.5s;}" +
                " @media only screen and (max-width: 768px) { form {max-width: 100%;}} " +
                "</style><title>NanoFramework</title></head>";
        }
    }
}
