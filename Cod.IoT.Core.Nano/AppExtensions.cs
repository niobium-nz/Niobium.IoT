using nanoFramework.Logging;
using nanoFramework.Logging.Debug;

namespace Cod.IoT.Nano
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        public static IApp AddCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                LogDispatcher.LoggerFactory = new DebugLoggerFactory();
                LoggerFactory.Initialize(name => LogDispatcher.LoggerFactory.CreateLogger(name));

                JSON.Instance = new NanoJsonSerializer();

                Cod.IoT.AppExtensions.AddCore(app, enableCommandSupport);

                isLoaded = true;
            }

            return app;
        }
    }
}
