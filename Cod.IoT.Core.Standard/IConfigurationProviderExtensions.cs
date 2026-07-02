using System;

namespace Cod.IoT
{
    public static class IConfigurationProviderExtensions
    {
        public static T Get<T>(this IConfigurationProvider configuration, string key) where T : IConvertible
        {
            var setting = configuration.GetAsObject(key);
            if (setting is T t)
            {
                return t;
            }
            
            try
            {
                return (T)Convert.ChangeType(setting, typeof(T));
            }
            catch (InvalidCastException)
            {
                return default;
            }
        }
    }
}
