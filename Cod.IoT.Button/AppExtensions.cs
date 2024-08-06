namespace Cod.IoT.Button
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        public static IComponent AutoStartInitiator { get; private set; }

        public static IApp UseButton(this IApp app, bool autoStartReadingGPIO = true, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                AutoStartInitiator = new AutoStartInitiator();

                app.UseCore(enableCommandSupport)
                   .RegisterService(new ButtonService());

                if (autoStartReadingGPIO)
                {
                    app.RegisterComponent(AutoStartInitiator);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
