using System.Collections;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace Cod.IoT.Networking.Web
{
    public static class HttpListenerRequestExtensions
    {
        public static string GetPath(this HttpListenerRequest request)
        {
            return request.RawUrl.Split('?')[0];
        }

        public static string GetQueryString(this HttpListenerRequest request)
        {
            var segments = request.RawUrl.Split('?');
            if (segments.Length > 1)
            {
                return segments[1];
            }

            return string.Empty;
        }

        public static Hashtable GetQueryParams(this HttpListenerRequest request)
        {
            var query = request.GetQueryString();
            return ParseParams(query);
        }

        public static Hashtable GetFormParams(this HttpListenerRequest request)
        {
            byte[] buffer = new byte[request.InputStream.Length];
            request.InputStream.Read(buffer, 0, (int)request.InputStream.Length);
            var requestBody = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return ParseParams(requestBody);
        }

        private static Hashtable ParseParams(string rawParams)
        {
            Hashtable hash = new Hashtable();

            string[] parPairs = rawParams.Split('&');
            foreach (string pair in parPairs)
            {
                string[] nameValue = pair.Split('=');
                hash.Add(nameValue[0], nameValue[1]);
            }

            return hash;
        }
    }
}
