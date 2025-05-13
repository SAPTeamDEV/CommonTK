using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPTeam.CommonTK;

public class SettingsStore : SettingNode
{
    public SettingsStore() : base(null, "") { }

    public SettingAccessor this[string path] => new(GetSetting(path));

    public string ExportJson()
    {
        Dictionary<string, string> dict = Export();
        return JsonSerializer.Serialize(dict, typeof(Dictionary<string, string>), SettingsJsonContext.Default);
    }

    public void ImportJson(string json)
    {
        Dictionary<string, string> dict = JsonSerializer.Deserialize(json, typeof(Dictionary<string, string>), SettingsJsonContext.Default) as Dictionary<string, string>
            ?? throw new InvalidOperationException("Invalid JSON");

        Import(dict);
    }

    public void Import(Dictionary<string, string> settings)
    {
        foreach (KeyValuePair<string, string> kvp in settings)
        {
            UpdateSetting(kvp.Key, kvp.Value, true);
        }
    }

    public Dictionary<string, string> Export()
    {
        Dictionary<string, string> dict = [];
        foreach (Setting setting in GetSettings())
        {
            dict[setting.FullPath] = setting.RawValue;
        }

        return dict;
    }

    public readonly struct SettingAccessor
    {
        private readonly Setting _setting;

        internal SettingAccessor(Setting setting) => _setting = setting;

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
