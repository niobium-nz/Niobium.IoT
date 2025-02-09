namespace Cod.IoT.Button
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;
        public static IComponent AutoStartInitiator { get; private set; }
        public static IComponent ResetTrigger { get; private set; }

        public static IApp UseButton(this IApp app,
            bool autoStartReadingGPIO = true,
            bool enableCommandSupport = true,
            int resetButtonPin = 0,
            byte resetButtonPressPinValue = Constants.LowPinValue)
        {
            if (!isLoaded)
            {
                if (autoStartReadingGPIO)
                {
                    AutoStartInitiator ??= new AutoStartInitiator();
                }

                if (resetButtonPin > 0)
                {
                    ResetTrigger ??= new ResetTrigger(resetButtonPin, resetButtonPressPinValue);
                }

                app.UseCore(enableCommandSupport)
                   .RegisterService(new ButtonService());

                if (AutoStartInitiator != null)
                {
                    app.RegisterComponent(AutoStartInitiator);
                }

                if (ResetTrigger != null)
                {
                    app.RegisterComponent(ResetTrigger);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
