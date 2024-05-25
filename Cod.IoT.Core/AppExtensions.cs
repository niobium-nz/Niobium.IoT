namespace Cod.IoT
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        private static bool isCommandServiceLoaded = false;

        public static IApp AddCore(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.RegisterService(Constants.ConfigurationProviderID, new ConfigurationProvider());
                if (enableCommandSupport)
                {
                    app.AddCommand(new PingCommand());
                }

                isLoaded = true;
            }

            return app;
        }

        public static IApp AddCommand(this IApp app, ICommand command)
        {
            if (!isCommandServiceLoaded)
            {
                app.RegisterService(Constants.CommandServiceID, new CommandService());
                isCommandServiceLoaded = true;
            }

            ICommandService commandService = (ICommandService)app.GetService(Constants.CommandServiceID);
            commandService.RegisterCommand(command);

            return app;
        }
    }
}
