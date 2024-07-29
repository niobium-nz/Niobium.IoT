using nanoFramework.Hardware.Esp32;
using nanoFramework.Logging;
using nanoFramework.Logging.Serial;

namespace Cod.IoT.Core.Debug
{
    public static class AppExtensions
    {
        private static bool isLoaded = false;

        public static IApp UseCoreDebug(this IApp app, int txPin, int rxPin, bool enableCommandSupport = true)
        {
            if (!isLoaded)
            {
                Configuration.SetPinFunction(rxPin, DeviceFunction.COM3_RX);
                Configuration.SetPinFunction(txPin, DeviceFunction.COM3_TX);
                app.UseCore(enableCommandSupport);
                LogDispatcher.LoggerFactory = new SerialLoggerFactory("COM3", 9600);
                isLoaded = true;
            }

            return app;
        }
    }
}
