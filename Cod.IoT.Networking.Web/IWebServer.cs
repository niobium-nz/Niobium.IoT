namespace Cod.IoT.Networking.Web
{
    public interface IWebServer : IService
    {
        void RegisterHandler(IHttpHandler handler);

        void UnregisterHandler(IHttpHandler handler);

        bool IsRunning { get; }

        bool Start(int port, string ip);

        void Stop();
    }
}
