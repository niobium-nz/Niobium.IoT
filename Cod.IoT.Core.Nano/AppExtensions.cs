using nanoFramework.Logging;
using nanoFramework.Logging.Debug;

namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static ICommand PingCommand => Cod.IoT.Core.AppExtensions.PingCommand;

        public static bool IsCommandSupportEnabled => Cod.IoT.Core.AppExtensions.IsCommandSupportEnabled;

        public static IApp UseCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                LogDispatcher.LoggerFactory = new DebugLoggerFactory();
                LoggerFactory.Initialize(name => LogDispatcher.LoggerFactory.CreateLogger(name));

                JSON.Instance = new NanoJsonSerializer();

                Cod.IoT.Core.AppExtensions.UseCore(app, enableCommandSupport);

                isLoaded = true;
            }

            return app;
        }

        public static IApp RegisterCommand(this IApp app, ICommand command)
            => Cod.IoT.Core.AppExtensions.RegisterCommand(app, command);
    }
}
