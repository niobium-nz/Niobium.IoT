using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Runtime.InteropServices;
using System.Security.Authentication;

namespace Cod.IoT
{
    public static class ExceptionExtensions
    {
        public static List<Type> NetworkExceptions { get; } =
        [
            typeof(IOException),
            typeof(SocketException),
            typeof(TimeoutException),
            typeof(OperationCanceledException),
            typeof(HttpRequestException),
            typeof(WebException),
            typeof(WebSocketException),
        ];

        private static bool IsNetwork(Exception singleException) => NetworkExceptions.Any(baseExceptionType => baseExceptionType.IsInstanceOfType(singleException));

        private static bool IsTlsSecurity(Exception singleException)
        {
            if (// WinHttpException (0x80072F8F): A security error occurred.
                (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (singleException.HResult == unchecked((int)0x80072F8F))) ||
                // CURLE_SSL_CACERT (60): Peer certificate cannot be authenticated with known CA certificates.
                (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && (singleException.HResult == 60)) ||
                singleException is AuthenticationException)
            {
                return true;
            }

            return false;
        }

        public static bool IsNetworkExceptionChain(this Exception exceptionChain) => exceptionChain.Unwind(true).Any(e => IsNetwork(e) && !IsTlsSecurity(e));

        public static bool IsSecurityExceptionChain(this Exception exceptionChain) => exceptionChain.Unwind(true).Any(e => IsTlsSecurity(e));

        public static IEnumerable<Exception> Unwind(this Exception exception, bool unwindAggregate = false)
        {
            while (exception != null)
            {
                yield return exception;

                if (!unwindAggregate)
                {
                    exception = exception.InnerException;
                    continue;
                }

                if (exception is AggregateException aggEx
                    && aggEx.InnerExceptions != null)
                {
                    foreach (var ex in aggEx.InnerExceptions)
                    {
                        foreach (var innerEx in ex.Unwind(true))
                        {
                            yield return innerEx;
                        }
                    }
                }

                exception = exception.InnerException;
            }
        }
    }
}
