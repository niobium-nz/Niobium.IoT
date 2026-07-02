using System;

namespace Cod.IoT
{
    public interface IJsonSerializer
    {
        string Serialize(object obj);

        object Deserialize(string json, Type type);
    }
}
