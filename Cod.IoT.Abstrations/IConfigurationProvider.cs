using System.Collections;

namespace Cod.IoT
{
    public interface IConfigurationProvider : IService
    {
        IEnumerable Keys { get; }

        object GetAsObject(string key);

        int GetAsNumber(string key);

        string GetAsString(string key);

        void Set(string key, object value);

        void Remove(string key);

        void Save();
    }
}
