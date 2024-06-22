using System;
using System.Net;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;

namespace Cod.IoT.Networking.Web
{
    public abstract class GenericHttpHandler : IHttpHandler
    {
        protected IWebServer Server { get; private set; }

        protected abstract string Method { get; }

        protected ILogger Logger { get; private set; }

        public bool IsInitialized { get; private set; }

        public void Initialize(IWebServer server)
        {
            Server = server;
            Logger ??= LogDispatcher.LoggerFactory.CreateLogger(this.GetType().Name);
            Initialize();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public virtual void PostHandle() { }

        public bool Handle(HttpListenerContext context)
        {
            if (context.Request.HttpMethod.ToUpper() != Method.ToUpper()
                || !IsSupported(context.Request.GetPath()))
            {
                return false;
            }

            Handle(context.Request, context.Response);
            return true;
        }

        protected abstract void Handle(HttpListenerRequest request, HttpListenerResponse response);

        protected object GetService(ushort id)
        {
            return Server.App.GetService(id);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Server = null;
            }
        }

        protected abstract bool IsSupported(string path);
        protected virtual void Initialize() { }
    }
}
