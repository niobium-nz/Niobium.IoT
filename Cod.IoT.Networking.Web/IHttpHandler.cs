using System;
using System.Net;

namespace Cod.IoT.Networking.Web
{
    public interface IHttpHandler : IDisposable
    {
        bool IsInitialized { get; }

        void Initialize(IWebServer server);

        bool Handle(HttpListenerContext context);

        void PostHandle();
    }
}
