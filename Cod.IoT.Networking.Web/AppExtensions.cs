namespace Cod.IoT.Networking.Web
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddWeb(this IApp app, IHttpHandler[] httpHandlers)
        {
            if (!isLoaded)
            {
                app.AddCore();

                var server = new WebServer();
                app.RegisterService(server);

                if (httpHandlers != null && httpHandlers.Length > 0)
                {
                    foreach (var handler in httpHandlers)
                    {
                        server.RegisterHandler(handler);
                    }
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
