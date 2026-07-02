using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    internal class LoggerFactoryAdaptor(ILoggerFactory loggerFactory) : ILoggerFactory
    {
        public void AddProvider(ILoggerProvider provider) => loggerFactory.AddProvider(provider);
        public ILogger CreateLogger(string categoryName) => loggerFactory.CreateLogger(categoryName);
        public void Dispose() => loggerFactory.Dispose();
    }
}
