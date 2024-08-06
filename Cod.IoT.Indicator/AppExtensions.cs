namespace Cod.IoT.Indicator
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IIndicatorService IndicatorService { get; private set; }

        public static IApp UseIndicator(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                IndicatorService = new IndicatorService();

                app.UseCore(enableCommandSupport)
                   .RegisterService(IndicatorService);

                isLoaded = true;
            }

            return app;
        }
    }
}
