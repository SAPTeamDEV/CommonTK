using System;

namespace SAPTeam.CommonTK;

public class Setting<T> : Setting
{
    private string _rawValue;
    private T _cachedValue;
    private string _cachedRaw;

    public override string RawValue
    {
        get => _rawValue;

        set
        {
            if (Parent is not null and SettingNode settingNode)
            {
                value = settingNode.ApplyInterceptors(Name, value);
            }

            if (TryParse<T>(value, out T? result))
            {
                _rawValue = value;
                UpdateCache(result);
            }

            throw new ArgumentException($"Invalid value '{value}' for setting '{FullPath}'.");
        }
    }

    public override object? ValueObject => Value;

    public override Type ValueType => typeof(T);

    public T Value
    {
        get
        {
            if (RawValue == _cachedRaw)
            {
                return _cachedValue;
            }

            if (TryParse<T>(RawValue, out T? result))
            {
                UpdateCache(result);
                return result;
            }

            throw new InvalidCastException($"Cannot parse setting '{FullPath}' value '{RawValue}' as {typeof(T).Name}.");
        }

        set
        {
            string strValue = value?.ToString() ?? throw new ArgumentNullException("Setting value cannot be null");
            RawValue = strValue;
        }
    }

    private void UpdateCache(T result)
    {
        _cachedValue = result;
        _cachedRaw = RawValue;
    }

    public Setting(SettingNode? parent, string name, T defaultValue, string description = "") : base(parent, name, defaultValue?.ToString() ?? throw new ArgumentNullException(nameof(defaultValue)), description) => UpdateCache(defaultValue);
}

public abstract class Setting : Member
{
    public virtual string RawValue { get; set; }

    public abstract object? ValueObject { get; }

    public abstract Type ValueType { get; }

    public string DefaultValue { get; }

    public string Description { get; }

    protected Setting(SettingNode? parent, string name, string rawValue, string description = "")
        : base(parent, name)
    {
        DefaultValue = rawValue;
        RawValue = rawValue;
        Description = description;
    }

    private static T ChangeType<T>(object obj) => obj is T t ? t : (T)obj;

    public T As<T>()
    {
        return TryParse<T>(RawValue, out T? result)
            ? result
            : throw new InvalidCastException($"Cannot parse setting '{FullPath}' value '{RawValue}' as {typeof(T).Name}.");
    }

    public static bool TryParse<T>(string rawValue, out T result)
    {
        Type type = typeof(T);
        result = default!;

        if (type == typeof(string))
        {
            result = ChangeType<T>(rawValue);
            return true;
        }

        if (type == typeof(bool) && bool.TryParse(rawValue, out bool b))
        {
            result = ChangeType<T>(b);
            return true;
        }

        if (type == typeof(int) && int.TryParse(rawValue, out int i))
        {
            result = ChangeType<T>(i);
            return true;
        }

        if (type.IsEnum)
        {
            try
            {
                result = (T)Enum.Parse(type, rawValue, ignoreCase: true);
                return true;
            }
            catch { }
        }

        return false;
    }
}
