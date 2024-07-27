using nanoFramework.Logging.Debug;
using nanoFramework.Logging;

namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        private static bool isCommandServiceLoaded = false;

        public static ICommand PingCommand { get; private set; } = new PingCommand();

        public static bool IsCommandSupportEnabled { get; private set; }

        public static IApp AddCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                LogDispatcher.LoggerFactory = new DebugLoggerFactory();

                IsCommandSupportEnabled = enableCommandSupport;
                app.RegisterService(new ConfigurationProvider());
                if (enableCommandSupport)
                {
                    app.RegisterCommand(PingCommand);
                }

                isLoaded = true;
            }

            return app;
        }

        public static IApp RegisterCommand(this IApp app, ICommand command)
        {
            if (!isCommandServiceLoaded)
            {
                app.RegisterService(new CommandService())
                   .RegisterService(new TaskService());

                isCommandServiceLoaded = true;
            }

            ICommandService commandService = (ICommandService)app.GetService(Constants.CommandServiceID);
            commandService.RegisterCommand(command);

            return app;
        }
    }
}
