using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cod.IoT
{
    internal class DateTimeToLongConverter : JsonConverter<long>
    {
        private readonly static JsonConverter<long> defaultConverter =
            (JsonConverter<long>)JsonSerializerOptions.Default.GetConverter(typeof(long));
        
        public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetDateTimeOffset(out var dateTimeOffset))
                {
                    return dateTimeOffset.ToUnixTimeMilliseconds();
                }
                else
                {
                    return 0;
                }
            }

            return defaultConverter.Read(ref reader, typeToConvert, options);
        }

        public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
        {
            defaultConverter.Write(writer, value, options);
        }
    }
}
