namespace Cod.IoT
{
    public static class IJsonSerializerExtensions
    {
        public static T Deserialize<T>(this IJsonSerializer serializer, string json)
        {
            var result = serializer.Deserialize(json, typeof(T));
            if (result != null && result is T t)
            {
                return t;
            }

            return default;
        }
    }
}
