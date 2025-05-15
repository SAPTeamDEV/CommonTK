// ----------------------------------------------------------------------------
//  <copyright file="SettingNode.cs" company="SAP Team" author="Alireza Poodineh">
//      Copyright © SAP Team
//      Released under the MIT License. See LICENSE.md.
//  </copyright>
// ----------------------------------------------------------------------------

namespace SAPTeam.CommonTK.Hierarchy;

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

    private readonly List<Interceptor> interceptors = [];

    /// <summary>
    /// Gets the pending settings that are queued for later update.
    /// </summary>
    /// <remarks>
    /// All settings that imported, but not yet created are stored here for automatic update right after setting creation.
    /// These settings are not yet part of the node and after creation, they will be removed from this list.
    /// </remarks>
    public Dictionary<string, string> PendingSettings { get; } = new(StringComparer.OrdinalIgnoreCase);

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

            IEnumerable<KeyValuePair<string, string>> pending = PendingSettings
                .Where(kvp => ParsePath(kvp.Key).Length > 1 && ParsePath(kvp.Key)[0] == parts[0]);

            foreach (KeyValuePair<string, string> kvp in pending)
            {
                PendingSettings.Remove(kvp.Key);
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

        if (PendingSettings.TryGetValue(parts[0], out var pendingValue))
        {
            UpdateSetting([parts[0]], pendingValue, false);
            PendingSettings.Remove(parts[0]);
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
    public void AddInterceptor(Interceptor interceptor) => interceptors.Add(interceptor);

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
                PendingSettings[string.Join(".", parts)] = value;
            }
            else
            {
                throw;
            }
        }
    }

    internal string ApplyInterceptors(string path, string value)
    {
        var current = value;

        foreach (Interceptor interceptor in interceptors)
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

        foreach (KeyValuePair<string, string> pending in PendingSettings)
        {
            dict[ToAbsolutePath(pending.Key)] = pending.Value;
        }

        foreach (SettingNode node in Nodes.OfType<SettingNode>())
        {
            foreach (KeyValuePair<string, string> pending in node.GetAllPendingSettings())
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
