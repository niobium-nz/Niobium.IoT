using System;
using nanoFramework.Json;

namespace Cod.IoT
{
    internal class NanoJsonSerializer : IJsonSerializer
    {
        public object Deserialize(string json, Type type)
        {
            nanoFramework.Json.Configuration.Settings.CaseSensitive = false;
            return JsonConvert.DeserializeObject(json, type);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
