using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    public static class LoggerFactory
    {
        private static ILoggerFactory _factory;

        public static void Initialize(ILoggerFactory factory)
        {
            _factory = factory;
        }

        public static ILogger CreateLogger(string name)
        {
            return _factory.CreateLogger(name);
        }
    }
}
