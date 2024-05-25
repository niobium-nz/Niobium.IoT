using Microsoft.Extensions.Logging;
using nanoFramework.Json;
using System;
using System.Collections;
using System.IO;

namespace Cod.IoT
{
    public class ConfigurationProvider : GenericService, IConfigurationProvider
    {
        private Hashtable configurations = new();

        public override ushort ID => Constants.ConfigurationProviderID;

        public IEnumerable Keys => configurations.Keys;

        public object GetAsObject(string key)
        {
            return configurations.Contains(key) ? configurations[key] : null;
        }

        public string GetAsString(string key)
        {
            return configurations.Contains(key) ? configurations[key] as string : null;
        }

        public void Set(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Remove(key);
            configurations.Add(key, value);
        }

        public void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (configurations.Contains(key))
            {
                configurations.Remove(key);
            }
        }

        public void Save()
        {
            try
            {
                File.WriteAllText(Constants.AppSettingFile, JsonConvert.SerializeObject(configurations));
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"An error occurred while saving app settings.");
            }
        }

        protected override void Initialize()
        {
            File.Delete(Constants.AppSettingFile);
            if (File.Exists(Constants.AppSettingFile))
            {
                configurations = (Hashtable)JsonConvert.DeserializeObject(File.ReadAllText(Constants.AppSettingFile), typeof(Hashtable));
            }
            else
            {
                Rollback();
            }
        }

        protected override void Rollback()
        {
            File.WriteAllText(Constants.AppSettingFile, $"{{\"App\":\"{App.GetFullName()}\"}}");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Save();
                configurations.Clear();
                configurations = null;
            }

            base.Dispose(disposing);
        }
    }
}
