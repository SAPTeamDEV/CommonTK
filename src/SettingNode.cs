using System;
using System.Collections.Generic;
using System.Linq;

namespace SAPTeam.CommonTK;

public class SettingNode : Node
{
    public delegate string Interceptor(string path, string currentValue);

    private readonly List<Interceptor> _interceptors = [];
    private readonly Dictionary<string, string> _pendingSettings = new(StringComparer.OrdinalIgnoreCase);

    public SettingNode(SettingNode? parent, string name) : base(parent, name)
    {

    }

    public Setting CreateSetting<T>(string path, T defaultValue, string description = "") => CreateSetting(ParsePath(path), defaultValue, description);

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

    public void AddInterceptor(Interceptor interceptor) => _interceptors.Add(interceptor);

    public Setting GetSetting(string path) => GetSetting(ParsePath(path));

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

    public Setting<T> GetSetting<T>(string path) => GetSetting<T>(ParsePath(path));

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

    public void UpdateSetting(string path, string value, bool queueIfNotFound = false) => UpdateSetting(ParsePath(path), value, queueIfNotFound);

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
            current = settingNode.ApplyInterceptors(string.Join(".", Name, path), current);
        }

        return current;
    }

    public IEnumerable<Setting> GetSettings() => GetMembers().OfType<Setting>();

    public IEnumerable<Setting<T>> GetSettings<T>() => GetMembers().OfType<Setting<T>>();
}
