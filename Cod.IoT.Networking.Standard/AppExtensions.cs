namespace Cod.IoT.Networking
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp UseNetwork(this IApp app, IServiceProvider serviceProvider)
        {
            if (!isLoaded)
            {
                app.UseCore(serviceProvider);

                isLoaded = true;
            }

            return app;
        }
    }
}
