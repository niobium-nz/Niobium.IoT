using nanoFramework.Logging;
using nanoFramework.Logging.Debug;

namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IConfigurationProvider ConfigurationProvider { get; private set; }
        public static ITaskService TaskService { get; private set; }
        public static ICommandService CommandService { get; private set; }
        public static IAction PingAction { get; private set; }

        public static bool IsCommandSupportEnabled { get; private set; }

        public static IApp UseCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                Constants.AppSettingFile = @"I:\appsettings.config";
                Constants.ExtensionFolder = @"I:\extensions";
                if (LogDispatcher.LoggerFactory == null)
                {
                    LogDispatcher.LoggerFactory = new DebugLoggerFactory();
                    LoggerFactory.Initialize(LogDispatcher.LoggerFactory);
                }

                JSON.Instance = new NanoJsonSerializer();

                IsCommandSupportEnabled = enableCommandSupport;

                ConfigurationProvider = new ConfigurationProvider();
                TaskService = new TaskService();
                CommandService = new CommandService();
                PingAction = new PingAction();

                app.RegisterService(ConfigurationProvider)
                    .RegisterAction(PingAction);

                isLoaded = true;
            }

            return app;
        }

        public static IApp RegisterAction(this IApp app, IAction action)
        {
            if (IsCommandSupportEnabled)
            {
                app.RegisterService(CommandService)
                   .RegisterService(TaskService);

                CommandService.RegisterAction(action);
            }

            return app;
        }
    }
}
