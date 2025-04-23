using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SAPTeam.CommonTK
{
    public class SettingsStore : SettingNode
    {
        public SettingsStore() : base(null, "") { }

        public SettingAccessor this[string path] => new(GetSetting(path));

        public string ExportJson()
        {
            var dict = Export();
            return JsonSerializer.Serialize(dict, typeof(Dictionary<string, string>), SettingsJsonContext.Default);
        }

        public void ImportJson(string json)
        {
            var dict = JsonSerializer.Deserialize(json, typeof(Dictionary<string, string>), SettingsJsonContext.Default) as Dictionary<string, string>
                ?? throw new InvalidOperationException("Invalid JSON");

            Import(dict);
        }

        public void Import(Dictionary<string, string> settings)
        {
            foreach (var kvp in settings)
            {
                if (TryGetSetting(kvp.Key, out var setting))
                {
                    setting.RawValue = kvp.Value;
                }
            }
        }

        public Dictionary<string, string> Export()
        {
            var dict = new Dictionary<string, string>();
            foreach (var setting in GetSettings())
            {
                dict[setting.FullPath] = setting.RawValue;
            }
            return dict;
        }

        public readonly struct SettingAccessor
        {
            private readonly Setting _setting;

            internal SettingAccessor(Setting setting)
            {
                _setting = setting;
            }

            public static implicit operator bool(SettingAccessor a) => a.As<bool>();

            public static implicit operator int(SettingAccessor a) => a.As<int>();

            public static implicit operator string(SettingAccessor a) => a.As<string>();

            public T As<T>() => _setting.As<T>();
        }
    }

    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class SettingsJsonContext : JsonSerializerContext
    {
    }
}
