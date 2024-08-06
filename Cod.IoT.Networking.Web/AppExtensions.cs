namespace Cod.IoT.Networking.Web
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IWebServer WebServer { get; private set; }

        public static IApp UseWeb(this IApp app, IHttpHandler[] httpHandlers)
        {
            if (!isLoaded)
            {
                app.UseCore();

                WebServer ??= new WebServer();
                app.RegisterService(WebServer);

                if (httpHandlers != null && httpHandlers.Length > 0)
                {
                    foreach (var handler in httpHandlers)
                    {
                        WebServer.RegisterHandler(handler);
                    }
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
