using System;
using System.Collections;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Cod.IoT
{
    public class ConfigurationProvider : GenericService, IConfigurationProvider
    {
        private const string EmptySetting = "{}";
        private const string AppKey = "App";

        private ArrayList keys = new();
        private Hashtable configurations = new();

        public event EventHandler Cleared;

        public override int ID => Constants.ConfigurationProviderID;

        public IEnumerable Keys => keys;

        public ConfigurationProvider()
        {
            if (File.Exists(Constants.AppSettingFile))
            {
                try
                {
                    var json = File.ReadAllText(Constants.AppSettingFile);
                    Logger.LogInformation($"loading: {json}");
                    configurations = (Hashtable)JSON.Instance.Deserialize(json, typeof(Hashtable));
                    foreach (string key in configurations.Keys)
                    {
                        keys.Add(key);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogCritical(ex, $"An error occurred while saving app settings.");
                    Rollback();
                }
            }
            else
            {
                Rollback();
            }
        }

        protected override void Initialize()
        {
            if (GetAsString(AppKey) == null)
            {
                Set(AppKey, App.GetFullName());
            }
        }

        public virtual bool GetAsBoolean(string key)
        {
            return keys.Contains(key) ? (bool)configurations[key] : false;
        }

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
                var json = JSON.Instance.Serialize(configurations);
                Logger.LogInformation($"saving: {json}");
                File.WriteAllText(Constants.AppSettingFile, json);
            }
            catch (Exception ex)
            {
                Logger.LogCritical(ex, $"An error occurred while saving app settings.");
            }
        }

        public void Clear()
        {
            ClearCore();
            OnCleared();
        }

        protected virtual void ClearCore()
        {
            configurations?.Clear();
            keys?.Clear();
            File.WriteAllText(Constants.AppSettingFile, EmptySetting);
            Logger.LogInformation("Configuration has been cleared.");
        }

        protected virtual void OnCleared()
        {
            Cleared?.Invoke(this, EventArgs.Empty);
        }

        protected override void Rollback()
        {
            Logger.LogWarning("Config file not found, rolling back...");
            ClearCore();
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
