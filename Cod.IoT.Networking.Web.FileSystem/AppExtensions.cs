namespace Cod.IoT.Networking.Web.FileSystem
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        public static IHttpHandler FileSystemHttpHandler { get; private set; }

        public static IApp AddWeb(this IApp app, string wwwroot, IFileBasedWWWResourceProvider wwwProvider, IHttpHandler[] httpHandlers = null)
        {
            if (!isLoaded)
            {
                app.AddWeb(httpHandlers);

                FileSystemHttpHandler ??= new FileSystemHttpHandler(wwwroot, wwwProvider);

                var server = (IWebServer)app.GetService(Constants.WebServerID);
                server.RegisterHandler(FileSystemHttpHandler);

                isLoaded = true;
            }

            return app;
        }
    }
}
