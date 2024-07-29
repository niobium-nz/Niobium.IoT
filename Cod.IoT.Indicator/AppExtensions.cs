namespace Cod.IoT.Indicator
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp UseIndicator(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.UseCore(enableCommandSupport)
                   .RegisterService(new IndicatorService());

                isLoaded = true;
            }

            return app;
        }
    }
}
