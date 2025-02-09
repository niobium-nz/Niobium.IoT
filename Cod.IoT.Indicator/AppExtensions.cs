namespace Cod.IoT.Indicator
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IIndicatorService IndicatorService { get; private set; }
        public static IComponent ResetIndicator { get; private set; }

        public static IApp UseIndicator(this IApp app, bool enableCommandSupport = true, int resetLEDPin = 0)
        {
            if (!isLoaded)
            {
                IndicatorService = new IndicatorService();

                if (resetLEDPin > 0)
                {
                    ResetIndicator ??= new ResetIndicator(resetLEDPin);
                }

                app.UseCore(enableCommandSupport)
                   .RegisterService(IndicatorService);

                if (ResetIndicator != null)
                {
                    app.RegisterComponent(ResetIndicator);
                }

                isLoaded = true;
            }

            return app;
        }
    }
}
