using System;

namespace Cod.IoT
{
    public static class IServiceExtensions
    {
        private const string DefaultExceptionMessageOnFailedCheck = "Further check failed on setting: {0}";

        public static T GetSetting<T>(this IConfigurationProvider configuration, string key, Func<T, bool> check = null, Func<T, string> exceptionMessageOnFailedCheck = null) where T : IConvertible
        {
            T result = configuration.Get<T>(key) ?? throw new ApplicationException($"Missing configuration: {key}");
            if (result is string str && string.IsNullOrWhiteSpace(str))
            {
                throw new ApplicationException($"Missing configuration: {key}");
            }

            if (check != null)
            {
                var pass = check(result);
                if (!pass)
                {
                    var msg = exceptionMessageOnFailedCheck(result) ?? string.Format(DefaultExceptionMessageOnFailedCheck, result);
                    throw new ApplicationException(msg);
                }
            }

            return result;
        }
    }
}
