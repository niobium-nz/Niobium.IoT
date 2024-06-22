namespace Cod.IoT.Indicator
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp AddIndicator(this IApp app, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                app.AddCore(enableCommandSupport)
                   .RegisterService(new IndicatorService());

                isLoaded = true;
            }

            return app;
        }
    }
}
