using Cod.IoT.Networking;

namespace Cod.IoT.Hub
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp UseHub(this IApp app, IServiceProvider serviceProvider)
        {
            if (!isLoaded)
            {
                app.UseNetwork(serviceProvider);

                isLoaded = true;
            }

            return app;
        }
    }
}
