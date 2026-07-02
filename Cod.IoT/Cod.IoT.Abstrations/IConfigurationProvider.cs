using System;
using System.Collections;

namespace Cod.IoT
{
    public interface IConfigurationProvider : IService
    {
        IEnumerable Keys { get; }

        event EventHandler Cleared;

        object GetAsObject(string key);

        bool GetAsBoolean(string key);

        int GetAsNumber(string key);

        string GetAsString(string key);

        void Set(string key, object value);

        void Remove(string key);

        void Clear();

        void Save();
    }
}
