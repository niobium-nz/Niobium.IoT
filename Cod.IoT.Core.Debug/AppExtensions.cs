#if DEBUG
using nanoFramework.Hardware.Esp32;
using nanoFramework.Logging;
using nanoFramework.Logging.Serial;
#endif

namespace Cod.IoT.Core.Debug
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp UseCoreDebug(this IApp app, int txPin, int rxPin, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
#if DEBUG
                Configuration.SetPinFunction(rxPin, DeviceFunction.COM3_RX);
                Configuration.SetPinFunction(txPin, DeviceFunction.COM3_TX);

                LogDispatcher.LoggerFactory = new SerialLoggerFactory("COM3", 9600);
                LoggerFactory.Initialize(LogDispatcher.LoggerFactory);
#endif

                app.UseCore(enableCommandSupport);
                isLoaded = true;
            }
            return app;
        }
    }
}
