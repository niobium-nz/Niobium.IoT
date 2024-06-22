using System;
using System.Collections;
using System.Net;
using System.Threading;
using Microsoft.Extensions.Logging;
using nanoFramework.Logging;

namespace Cod.IoT.Networking.Web
{
    public class WebServer : GenericService, IWebServer
    {
        protected readonly ArrayList handlers;
        protected HttpListener listener;
        protected Thread worker;
        private readonly ILogger logger;

        public WebServer()
        {
            handlers = new ArrayList();
            logger = LogDispatcher.LoggerFactory.CreateLogger(this.GetType().Name);
        }

        public virtual bool IsRunning => listener != null && listener.IsListening;

        public override int ID => Constants.WebServerID;

        public virtual void RegisterHandler(IHttpHandler handler)
        {
            if (handler != null && !handlers.Contains(handler))
            {
                handlers.Add(handler);
            }
        }

        public virtual void UnregisterHandler(IHttpHandler handler)
        {
            if (handler != null && handlers.Contains(handler))
            {
                handlers.Remove(handler);
            }
        }

        public virtual bool Start(int port, string ip)
        {
            if (IsStarted)
            {
                return true;
            }

            try
            {
                if (listener == null)
                {
                    listener = new HttpListener("http", port, IPAddress.Parse(ip));
                    listener.Start();

                    worker = new Thread(Server);
                    worker.Start();
                    return true;
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Failed to start WiFi provisioning web server.");
            }

            return false;
        }

        public virtual void Stop()
        {
            try
            {
                if (IsRunning)
                {
                    listener.Abort();
                }
                worker.Join(5000);
            }
            catch
            {
            }

            worker = null;
            listener = null;
        }

        protected override void Initialize()
        {
            if (handlers != null)
            {
                foreach (IHttpHandler handler in handlers)
                {
                    handler.Initialize(this);
                }
            }
        }

        protected virtual void Server()
        {
            while (listener.IsListening)
            {
                try
                {
                    var context = listener.GetContext();
                    if (context != null)
                    {
                        ProcessRequest(context);
                    }
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error occurred while handling WiFi provisioning request");
                }

                Thread.Sleep(100);
            }
        }

        protected virtual void ProcessRequest(HttpListenerContext context)
        {
            IHttpHandler targetHandler = null;

            try
            {
                foreach (IHttpHandler handler in handlers)
                {
                    var handled = handler.Handle(context);
                    if (handled)
                    {
                        targetHandler = handler;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Error occurred while handling HTTP request");
            }
            finally
            {
                context?.Response?.Close();
            }

            if (targetHandler != null)
            {
                try
                {
                    targetHandler.PostHandle();
                }
                catch (Exception e)
                {
                    logger.LogError(e, "Error occurred while post-handling HTTP request");
                }
            }
        }
    }
}
