using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents a node in a hierarchical structure that can contain settings.
/// </summary>
public class SettingNode : Node
{
    /// <summary>
    /// Encapsulates a method that intercepts the setting value before it is set.
    /// </summary>
    /// <param name="path">
    /// The relative path of the setting being set.
    /// </param>
    /// <param name="value">
    /// The value being set for the setting.
    /// </param>
    /// <returns>
    /// The modified value to be set for the setting.
    /// </returns>
    public delegate string Interceptor(string path, string value);

    private readonly List<Interceptor> _interceptors = [];
    private readonly Dictionary<string, string> _pendingSettings = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets the pending settings that are queued for later update.
    /// </summary>
    /// <remarks>
    /// All settings that imported, but not yet created are stored here for automatic update right after setting creation.
    /// These settings are not yet part of the node and after creation, they will be removed from this list.
    /// </remarks>
    public Dictionary<string, string> PendingSettings => _pendingSettings;

    /// <summary>
    /// Gets the settings of this node.
    /// </summary>
    public IEnumerable<Setting> Settings => Members.OfType<Setting>();

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingNode"/> class.
    /// </summary>
    /// <param name="parent">
    /// The parent node of this setting node. If null, this node is considered as root.
    /// </param>
    /// <param name="name">
    /// The name of the setting node.
    /// </param>
    public SettingNode(SettingNode? parent, string name) : base(parent, name)
    {

    }

    /// <inheritdoc/>
    protected override Node NewNode(string name) => new SettingNode(this, name);

    /// <summary>
    /// Creates a new setting at the specified path.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the setting value.
    /// </typeparam>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <param name="defaultValue">
    /// The default value of the setting.
    /// </param>
    /// <param name="description">
    /// The description of the setting.
    /// </param>
    /// <returns>
    /// The created setting.
    /// </returns>
    public Setting CreateSetting<T>(string path, T defaultValue, string description = "") => CreateSetting(ParsePath(path), defaultValue, description);

    /// <summary>
    /// Creates a new setting at the specified path components.
    /// </summary>
    /// <typeparam name="T">
    /// Type of the setting value.
    /// </typeparam>
    /// <param name="parts">
    /// The components of the relative path of the setting.
    /// </param>
    /// <param name="defaultValue">
    /// The default value of the setting.
    /// </param>
    /// <param name="description">
    /// The description of the setting.
    /// </param>
    /// <returns>
    /// The created setting.
    /// </returns>
    protected Setting CreateSetting<T>(string[] parts, T defaultValue, string description = "")
    {
        if (parts.Length > 1)
        {
            SettingNode child = (SettingNode)CreateNode([parts[0]]);

            IEnumerable<KeyValuePair<string, string>> pending = _pendingSettings
                .Where(kvp => ParsePath(kvp.Key).Length > 1 && ParsePath(kvp.Key)[0] == parts[0]);

            foreach (KeyValuePair<string, string> kvp in pending)
            {
                _pendingSettings.Remove(kvp.Key);
            }

            Dictionary<string[], string> formattedPending = pending.ToDictionary(kvp => ParsePath(kvp.Key).Skip(1).ToArray(), kvp => kvp.Value);

            foreach (KeyValuePair<string[], string> kvp in formattedPending)
            {
                child.UpdateSetting(kvp.Key, kvp.Value, true);
            }

            return child.CreateSetting(parts.Skip(1).ToArray(), defaultValue, description);
        }

        if (!TryGetSetting(parts[0], out Setting setting))
        {
            setting = new Setting<T>(this, parts[0], defaultValue, description);
            AddMember(setting);
        }

        if (_pendingSettings.TryGetValue(parts[0], out string? pendingValue))
        {
            UpdateSetting([parts[0]], pendingValue, false);
            _pendingSettings.Remove(parts[0]);
        }

        return setting;
    }

    /// <summary>
    /// Adds an interceptor to the setting node.
    /// The interceptor is called before the setting value is set.
    /// </summary>
    /// <remarks>
    /// The interceptor is called for all settings in the node and its children nodes.
    /// </remarks>
    /// <param name="interceptor">
    /// The interceptor method.
    /// </param>
    public void AddInterceptor(Interceptor interceptor) => _interceptors.Add(interceptor);

    /// <summary>
    /// Gets a setting at the specified path.
    /// </summary>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <returns>
    /// The setting at the specified path.
    /// </returns>
    public Setting GetSetting(string path) => GetSetting(ParsePath(path));

    /// <summary>
    /// Gets a setting at the specified path components.
    /// </summary>
    /// <param name="parts">
    /// The components of the relative path of the setting.
    /// </param>
    /// <returns>
    /// The setting at the specified path.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    protected Setting GetSetting(string[] parts)
    {
        if (parts.Length > 1)
        {
            SettingNode child = (SettingNode)GetNode([parts[0]]);
            return child.GetSetting(parts.Skip(1).ToArray());
        }

        return !TryGetMember(parts[0], out Member? member)
            ? throw new KeyNotFoundException($"Setting '{parts[0]}' not found under '{FullPath}'")
            : member is Setting setting
            ? setting
            : throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting");
    }

    /// <summary>
    /// Tries to get a setting at the specified path.
    /// </summary>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <param name="setting">
    /// The setting at the specified path, if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the setting was found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetSetting(string path, out Setting setting)
    {
        try
        {
            setting = GetSetting(path);
            return true;
        }
        catch
        {
            setting = null!;
            return false;
        }
    }

    /// <summary>
    /// Gets a setting with the <typeparamref name="T"/> value type at the specified path.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the setting value.
    /// </typeparam>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <returns>
    /// The setting with the <typeparamref name="T"/> value type at the specified path.
    /// </returns>
    public Setting<T> GetSetting<T>(string path) => GetSetting<T>(ParsePath(path));

    /// <summary>
    /// Gets a setting with the <typeparamref name="T"/> value type at the specified path components.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the setting value.
    /// </typeparam>
    /// <param name="parts">
    /// The components of the relative path of the setting.
    /// </param>
    /// <returns>
    /// The setting with the <typeparamref name="T"/> value type at the specified path.
    /// </returns>
    /// <exception cref="KeyNotFoundException"></exception>
    /// <exception cref="InvalidCastException"></exception>
    protected Setting<T> GetSetting<T>(string[] parts)
    {
        if (parts.Length > 1)
        {
            SettingNode child = (SettingNode)GetNode([parts[0]]);
            return child.GetSetting<T>(parts.Skip(1).ToArray());
        }

        return !TryGetMember(parts[0], out Member? member)
            ? throw new KeyNotFoundException($"Member '{parts[0]}' not found under '{FullPath}'")
            : member is Setting<T> setting
            ? setting
            : throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting with type '{typeof(T).Name}'");
    }

    /// <summary>
    /// Tries to get a setting with the <typeparamref name="T"/> value type at the specified path.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the setting value.
    /// </typeparam>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <param name="setting">
    /// The setting with the <typeparamref name="T"/> value type at the specified path, if found.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the setting was found; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryGetSetting<T>(string path, out Setting<T> setting)
    {
        try
        {
            setting = GetSetting<T>(path);
            return true;
        }
        catch
        {
            setting = null!;
            return false;
        }
    }

    /// <summary>
    /// Updates the setting at the specified path with the given value.
    /// </summary>
    /// <param name="path">
    /// The relative path of the setting.
    /// </param>
    /// <param name="value">
    /// The new value for the setting.
    /// </param>
    /// <param name="queueIfNotFound">
    /// If <see langword="true"/>, queues the setting for later update if not found.
    /// </param>
    public void UpdateSetting(string path, string value, bool queueIfNotFound = false) => UpdateSetting(ParsePath(path), value, queueIfNotFound);

    /// <summary>
    /// Updates the setting at the specified path components with the given value.
    /// </summary>
    /// <param name="parts">
    /// The components of the relative path of the setting.
    /// </param>
    /// <param name="value">
    /// The new value for the setting.
    /// </param>
    /// <param name="queueIfNotFound">
    /// If <see langword="true"/>, queues the setting for later update if not found.
    /// </param>
    protected void UpdateSetting(string[] parts, string value, bool queueIfNotFound)
    {
        try
        {
            if (parts.Length > 1)
            {
                SettingNode child = (SettingNode)GetNode([parts[0]]);
                child.UpdateSetting(parts.Skip(1).ToArray(), value, queueIfNotFound);
                return;
            }

            Member member = GetMember([parts[0]]);

            if (member is Setting setting)
            {
                setting.RawValue = value;
            }
            else
            {
                throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting");
            }
        }
        catch
        {
            if (queueIfNotFound)
            {
                _pendingSettings[string.Join(".", parts)] = value;
            }
            else
            {
                throw;
            }
        }
    }

    internal string ApplyInterceptors(string path, string value)
    {
        string current = value;

        foreach (Interceptor interceptor in _interceptors)
        {
            current = interceptor(path, current);
        }

        if (Parent is not null and SettingNode settingNode)
        {
            current = settingNode.ApplyInterceptors(JoinPath(Name, path), current);
        }

        return current;
    }

    /// <summary>
    /// Gets all pending settings in this node and in all child nodes.
    /// </summary>
    /// <returns>
    /// A dictionary of pending settings in this node and in all child nodes with their full paths as keys and their values as values.
    /// </returns>
    protected Dictionary<string, string> GetAllPendingSettings()
    {
        Dictionary<string, string> dict = [];

        foreach (var pending in _pendingSettings)
        {
            dict[ToAbsolutePath(pending.Key)] = pending.Value;
        }

        foreach (var node in Nodes.OfType<SettingNode>())
        {
            foreach (var pending in node.GetAllPendingSettings())
            {
                dict[pending.Key] = pending.Value;
            }
        }

        return dict;
    }

    /// <summary>
    /// Gets all settings in this node and its children nodes.
    /// </summary>
    /// <returns>
    /// An enumerable collection of settings in this node and its children nodes.
    /// </returns>
    public IEnumerable<Setting> GetAllSettings() => GetAllMembers().OfType<Setting>();

    /// <summary>
    /// Gets all settings with the <typeparamref name="T"/> value type in this node and its children nodes.
    /// </summary>
    /// <typeparam name="T">
    /// The type of the setting value.
    /// </typeparam>
    /// <returns>
    /// An enumerable collection of settings with the <typeparamref name="T"/> value type in this node and its children nodes.
    /// </returns>
    public IEnumerable<Setting<T>> GetAllSettings<T>() => GetAllMembers().OfType<Setting<T>>();
}
