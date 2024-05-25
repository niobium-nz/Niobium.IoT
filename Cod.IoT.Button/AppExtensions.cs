namespace Cod.IoT.Button
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddButton(this IApp app, bool autoStartReadingGPIO = true)
        {
            if (!isLoaded)
            {
                app.AddCore();
                app.RegisterService(Constants.ButtonServiceID, new ButtonService());
                if (autoStartReadingGPIO)
                {
                    app.RegisterComponent(new AutoStartInitiator());
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
