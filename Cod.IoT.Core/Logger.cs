using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    internal static class Logger
    {
        public static ILogger Instance { get; set; } = new DiagnosticLogger();
    }
}
