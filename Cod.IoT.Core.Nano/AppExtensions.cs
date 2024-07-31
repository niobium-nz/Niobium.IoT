using nanoFramework.Logging;
using nanoFramework.Logging.Debug;

namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IAction PingAction { get; private set; } = new PingAction();

        public static bool IsCommandSupportEnabled { get; private set; }

        public static IApp UseCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                IsCommandSupportEnabled = enableCommandSupport;
               
                LogDispatcher.LoggerFactory = new DebugLoggerFactory();
                LoggerFactory.Initialize(name => LogDispatcher.LoggerFactory.CreateLogger(name));
                JSON.Instance = new NanoJsonSerializer();

                app.RegisterService(new ConfigurationProvider());

                if (IsCommandSupportEnabled)
                {
                    app.RegisterAction(PingAction);
                }

                isLoaded = true;
            }

            return app;
        }

        public static IApp RegisterAction(this IApp app, IAction action)
        {
            if (!IsCommandSupportEnabled)
            {
                app.RegisterService(new CommandService())
                   .RegisterService(new TaskService());

                IsCommandSupportEnabled = true;
            }

            ICommandService commandService = (ICommandService)app.GetService(Constants.CommandServiceID);
            commandService.RegisterAction(action);

            return app;
        }
    }
}
