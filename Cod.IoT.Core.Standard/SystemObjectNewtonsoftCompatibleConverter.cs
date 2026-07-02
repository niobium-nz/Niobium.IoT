using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cod.IoT
{
    internal class SystemObjectNewtonsoftCompatibleConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt64(out long longValue))
                {
                    return longValue;
                }
                else
                {
                    if (reader.TryGetDecimal(out decimal decimalValue))
                    {
                        return decimalValue;
                    }
                }
            }
            else if (reader.TokenType == JsonTokenType.True)
            {
                return true;
            }
            else if (reader.TokenType == JsonTokenType.False)
            {
                return false;
            }
            else if (reader.TokenType == JsonTokenType.String)
            {
                if (reader.TryGetGuid(out Guid g))
                {
                    return g;
                }
                if (reader.TryGetDateTimeOffset(out DateTimeOffset datetimeoffset))
                {
                    return datetimeoffset;
                }
                if (reader.TryGetDateTime(out DateTime datetime))
                {
                    return datetime;
                }
                return reader.GetString();
            }

            // Use JsonElement as fallback.
            // Newtonsoft uses JArray or JObject.
            using (JsonDocument document = JsonDocument.ParseValue(ref reader))
            {
                return document.RootElement.Clone();
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            var valueType = value.GetType();
            var converter = JsonSerializerOptions.Default.GetConverter(valueType);
            var typedConverter = typeof(JsonConverter<>).MakeGenericType(valueType);
            var write = typedConverter.GetMethod(nameof(Write));
            write.Invoke(converter, [writer, value, options]);
        }
    }
}
