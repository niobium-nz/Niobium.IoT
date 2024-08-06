namespace Cod.IoT.Networking.Web
{
    public abstract class Constants : Cod.IoT.Networking.Constants
    {
        public const int WebServerID = 7;
        public const string HttpGetMethod = "GET";
        public const string HttpPostMethod = "POST";

        public const int HttpRequestMaxRetry = 5;
        public const int HttpRequestRetryInterval = 500;
    }
}
