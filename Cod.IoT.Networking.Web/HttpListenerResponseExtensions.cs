using System.IO;
using System.Net;
using System.Text;
using nanoFramework.Json;

namespace Cod.IoT.Networking.Web
{
    public static class HttpListenerResponseExtensions
    {
        public static void SendJsonResponse(this HttpListenerResponse response, object obj, HttpStatusCode status = HttpStatusCode.OK)
        {
            response.ContentType = "application/json";
            response.SendResponse(JsonConvert.SerializeObject(obj), status);
        }

        public static void SendResponse(this HttpListenerResponse response, Stream responseStream, HttpStatusCode status = HttpStatusCode.OK)
        {
            if (responseStream == null || responseStream.Length == 0)
            {
                return;
            }

            response.StatusCode = (int)status;
            response.ContentLength64 = responseStream.Length;

            var buff = new byte[1024];
            while (true)
            {
                var read = responseStream.Read(buff, 0, buff.Length);
                if (read > 0)
                {
                    response.OutputStream.Write(buff, 0, read);
                }

                if (read < buff.Length)
                {
                    break;
                }
            }
        }

        public static void SendResponse(this HttpListenerResponse response, string responseString, HttpStatusCode status = HttpStatusCode.OK)
        {
            response.ContentEncoding = Encoding.UTF8;
            response.SendResponse(Encoding.UTF8.GetBytes(responseString), status);
        }

        public static void SendResponse(this HttpListenerResponse response, byte[] responseBytes, HttpStatusCode status = HttpStatusCode.OK)
        {
            response.StatusCode = (int)status;
            response.ContentLength64 = responseBytes.Length;
            response.OutputStream.Write(responseBytes, 0, responseBytes.Length);
        }
    }
}
