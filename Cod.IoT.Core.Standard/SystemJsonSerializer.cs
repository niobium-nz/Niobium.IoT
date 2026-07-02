using System;
using System.Text.Json;

namespace Cod.IoT
{
    internal class SystemJsonSerializer : IJsonSerializer
    {
        private static readonly JsonSerializerOptions options;

        static SystemJsonSerializer()
        {
            options = new(JsonSerializerDefaults.Web)
            {
                IncludeFields = true,
                IgnoreReadOnlyProperties = true,
            };
            options.Converters.Add(new SystemObjectNewtonsoftCompatibleConverter());
            options.Converters.Add(new DateTimeToLongConverter());
        }

        public object Deserialize(string json, Type type)
        {
            return JsonSerializer.Deserialize(json, type, options);
        }

        public string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
