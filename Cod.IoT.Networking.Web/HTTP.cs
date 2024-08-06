using System;
using System.Net.Http;
using System.Net.Security;
using System.Text;
using System.Threading;

namespace Cod.IoT.Networking.Web
{
    public static class HTTP
    {
        private const string JSONMediaType = "application/json";
        public static HttpClient HttpClient { get; private set; }

        public static HttpResponse Get(string url)
        {
            HttpResponseMessage response = null;
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                response = Send(request, Constants.HttpRequestMaxRetry);
            }

            return response.ToResult();
        }

        public static HttpResponse Post(string url, object body)
        {
            HttpResponseMessage response = null;
            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                var json = JSON.Instance.Serialize(body);
                request.Content = new StringContent(json, Encoding.UTF8, JSONMediaType);
                response = Send(request, Constants.HttpRequestMaxRetry);
            }

            return response.ToResult();
        }

        private static HttpResponseMessage Send(HttpRequestMessage request, int remainingRetry)
        {
            if (remainingRetry <= 0)
            {
                return null;
            }

            HttpClient ??= new HttpClient() { SslVerification = SslVerification.NoVerification };

            try
            {
                return HttpClient.Send(request);
            }
            catch (Exception)
            {
                Thread.Sleep(Constants.HttpRequestRetryInterval);
                return Send(request, --remainingRetry);
            }
        }

        private static HttpResponse ToResult(this HttpResponseMessage response)
        {
            if (response == null)
            {
                return null;
            }

            using (response)
            {
                var result = new HttpResponse
                {
                    Body = response.Content.ReadAsString(),
                    Status = (int)response.StatusCode,
                };
                return result;
            }
        }
    }
}
