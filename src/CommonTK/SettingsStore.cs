// ----------------------------------------------------------------------------
//  <copyright file="SettingsStore.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;

using SAPTeam.CommonTK.Hierarchy;

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a settings store that can be used to manage application settings.
/// </summary>
public class SettingsStore : SettingNode
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsStore"/> class.
    /// </summary>
    public SettingsStore() : base(null, "") { }

    /// <summary>
    /// Gets the setting at the specified path.
    /// </summary>
    /// <param name="path">
    /// The path to the setting.
    /// </param>
    /// <returns>
    /// The setting at the specified path.
    /// </returns>
    public SettingAccessor this[string path] => new(GetSetting(path));

    /// <summary>
    /// Exports the settings to a JSON string.
    /// </summary>
    /// <param name="exportPending">
    /// If true, pending settings will be included in the exported JSON string.
    /// </param>
    /// <returns>
    /// A JSON string representing the settings.
    /// </returns>
    public string ExportJson(bool exportPending)
    {
        Dictionary<string, string> dict = Export(exportPending);
        return JsonSerializer.Serialize(dict, typeof(Dictionary<string, string>), SettingsJsonContext.Default);
    }

    /// <summary>
    /// Imports settings from a JSON string.
    /// </summary>
    /// <param name="json">
    /// The JSON string to import.
    /// </param>
    /// <exception cref="InvalidOperationException"></exception>
    public void ImportJson(string json)
    {
        Dictionary<string, string> dict = JsonSerializer.Deserialize(json, typeof(Dictionary<string, string>), SettingsJsonContext.Default) as Dictionary<string, string>
            ?? throw new InvalidOperationException("Invalid JSON");

        Import(dict);
    }

    /// <summary>
    /// Imports settings from a dictionary.
    /// </summary>
    /// <param name="settings">
    /// The dictionary containing the settings to import.
    /// </param>
    public void Import(Dictionary<string, string> settings)
    {
        foreach (KeyValuePair<string, string> kvp in settings)
        {
            UpdateSetting(kvp.Key, kvp.Value, true);
        }
    }

    /// <summary>
    /// Exports the settings to a dictionary.
    /// </summary>
    /// <param name="exportPending">
    /// If true, pending settings will be included in the exported dictionary.
    /// </param>
    /// <returns>
    /// A dictionary containing the settings.
    /// </returns>
    public Dictionary<string, string> Export(bool exportPending)
    {
        Dictionary<string, string> dict = [];

        foreach (Setting setting in GetAllSettings())
        {
            dict[setting.FullPath] = setting.RawValue;
        }

        if (exportPending)
        {
            foreach (KeyValuePair<string, string> pending in GetAllPendingSettings())
            {
                dict[pending.Key] = pending.Value;
            }
        }

        return dict;
    }

    /// <summary>
    /// Represents an accessor for a setting.
    /// </summary>
    public readonly struct SettingAccessor
    {
        private readonly Setting setting;

        internal SettingAccessor(Setting setting) => this.setting = setting;

        /// <summary>
        /// Updates the setting value with the specified raw value.
        /// </summary>
        /// <param name="value">
        /// The raw value to set.
        /// </param>
        public void Update(string value) => setting.RawValue = value;

        /// <summary>
        /// Resets the setting to its default value.
        /// </summary>
        public void Reset() => setting.Reset();

        /// <summary>
        /// Implicitly converts the setting value to a boolean value.
        /// </summary>
        /// <param name="accessor">
        /// The setting accessor.
        /// </param>
        public static implicit operator bool(SettingAccessor accessor) => accessor.To<bool>();

        /// <summary>
        /// Implicitly converts the setting value to an integer value.
        /// </summary>
        /// <param name="accessor">
        /// The setting accessor.
        /// </param>
        public static implicit operator int(SettingAccessor accessor) => accessor.To<int>();

        /// <summary>
        /// Implicitly converts the setting value to a double value.
        /// </summary>
        /// <param name="accessor">
        /// The setting accessor.
        /// </param>
        public static implicit operator string(SettingAccessor accessor) => accessor.To<string>();

        /// <summary>
        /// Converts the setting value to the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type to convert the setting value.
        /// </typeparam>
        /// <returns>
        /// The converted value of the setting.
        /// </returns>
        public T To<T>() => setting.To<T>();
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Dictionary<string, string>))]
internal partial class SettingsJsonContext : JsonSerializerContext
{
}
