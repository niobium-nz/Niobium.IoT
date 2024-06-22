using System;
using System.Collections;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using nanoFramework.Json;

namespace Cod.IoT
{
    public class ConfigurationProvider : GenericService, IConfigurationProvider
    {
        private ArrayList keys = new();
        private Hashtable configurations = new();

        public override int ID => Constants.ConfigurationProviderID;

        public IEnumerable Keys => keys;

        public virtual int GetAsNumber(string key)
        {
            return keys.Contains(key) ? (int)configurations[key] : 0;
        }

        public virtual object GetAsObject(string key)
        {
            return keys.Contains(key) ? configurations[key] : null;
        }

        public virtual string GetAsString(string key)
        {
            return keys.Contains(key) ? configurations[key] as string : null;
        }

        public virtual void Set(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            Remove(key);
            keys.Add(key);
            configurations.Add(key, value);
        }

        public virtual void Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            if (keys.Contains(key))
            {
                configurations.Remove(key);
                keys.Remove(key);
            }
        }

        public virtual void Save()
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
            if (File.Exists(Constants.AppSettingFile))
            {
                configurations = (Hashtable)JsonConvert.DeserializeObject(File.ReadAllText(Constants.AppSettingFile), typeof(Hashtable));
                foreach (string key in configurations.Keys)
                {
                    keys.Add(key);
                }
            }
            else
            {
                Rollback();
            }
        }

        protected override void Rollback()
        {
            var sb = new StringBuilder();
            sb.Append($"{{\"App\":\"{App.GetFullName()}\"");
            sb.Append("\"DevicePIN\":\"123\"");
            sb.Append("}");
            File.WriteAllText(Constants.AppSettingFile, sb.ToString());
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
