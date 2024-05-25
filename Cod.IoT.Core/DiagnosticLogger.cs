using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace Cod.IoT
{
    public class DiagnosticLogger : ILogger
    {
        public void Log(LogLevel logLevel, EventId eventId, string state, Exception exception, MethodInfo format)
        {
            string level = logLevel switch
            {
                LogLevel.Debug => "D",
                LogLevel.Information => "I",
                LogLevel.Warning => "W",
                LogLevel.Error => "E",
                LogLevel.Critical => "F",
                _ => "V",
            };

            Debug.WriteLine($"{DateTime.UtcNow.ToString("s")} [{level}] {state}");

            if (exception != null)
            {
                Debug.WriteLine($"* Exception: {exception.Message}\r\nSack = {exception.StackTrace}");
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }
    }
}
