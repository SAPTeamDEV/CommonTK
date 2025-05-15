using System;

namespace SAPTeam.CommonTK.Hierarchy;

/// <summary>
/// Represents a setting in a hierarchical structure.
/// </summary>
/// <typeparam name="T">
/// The type of the setting value.
/// </typeparam>
public class Setting<T> : Setting
{
    private string _rawValue;
    private T _cachedValue;
    private string _cachedRaw;

    /// <inheritdoc/>
    public override string RawValue
    {
        get => _rawValue;

        set
        {
            if (Parent is not null and SettingNode settingNode)
                value = settingNode.ApplyInterceptors(Name, value);

            if (TryParse(value, out T result))
            {
                _rawValue = value;
                UpdateCache(result);
            }

            throw new ArgumentException($"Invalid value '{value}' for setting '{FullPath}'.");
        }
    }

    /// <inheritdoc/>
    public override object? ValueObject => Value;

    /// <inheritdoc/>
    public override Type ValueType => typeof(T);

    /// <summary>
    /// Gets or sets the value of the setting.
    /// </summary>
    public T Value
    {
        get
        {
            if (RawValue == _cachedRaw)
                return _cachedValue;

            if (TryParse(RawValue, out T result))
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Setting{T}"/> class.
    /// </summary>
    /// <param name="parent">
    /// The parent node of this setting.
    /// </param>
    /// <param name="name">
    /// The name of the setting.
    /// </param>
    /// <param name="defaultValue">
    /// The default value of the setting.
    /// </param>
    /// <param name="description">
    /// The description of the setting.
    /// </param>
    /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Setting(SettingNode? parent, string name, T defaultValue, string description = "") : base(parent, name, defaultValue?.ToString() ?? throw new ArgumentNullException(nameof(defaultValue)), description) => UpdateCache(defaultValue);
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
}

/// <summary>
/// Represents a base class for settings.
/// </summary>
public abstract class Setting : Member
{
    /// <summary>
    /// Gets or sets the string value of the setting.
    /// </summary>
    public virtual string RawValue { get; set; }

    /// <summary>
    /// Gets the value of the setting as an object.
    /// </summary>
    public abstract object? ValueObject { get; }

    /// <summary>
    /// Gets the type of the setting value.
    /// </summary>
    public abstract Type ValueType { get; }

    /// <summary>
    /// Gets the default value of the setting as a string.
    /// </summary>
    public string DefaultValue { get; }

    /// <summary>
    /// Gets the description of the setting.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Setting"/> class.
    /// </summary>
    /// <param name="parent">
    /// The parent node of this setting.
    /// </param>
    /// <param name="name">
    /// The name of the setting.
    /// </param>
    /// <param name="rawValue">
    /// The default value of the setting as a string.
    /// </param>
    /// <param name="description">
    /// The description of the setting.
    /// </param>
    protected Setting(SettingNode? parent, string name, string rawValue, string description = "")
        : base(parent, name)
    {
        DefaultValue = rawValue;
        RawValue = rawValue;
        Description = description;
    }

    private static T ChangeType<T>(object obj) => obj is T t ? t : (T)obj;

    /// <summary>
    /// Converts the raw value of the setting to the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type to convert the raw value to.
    /// </typeparam>
    /// <returns>
    /// The converted value of the setting.
    /// </returns>
    /// <exception cref="InvalidCastException"></exception>
    public T To<T>()
    {
        return TryParse(RawValue, out T result)
            ? result
            : throw new InvalidCastException($"Cannot parse setting '{FullPath}' value '{RawValue}' as {typeof(T).Name}.");
    }

    /// <summary>
    /// Resets the setting to its default value.
    /// </summary>
    public void Reset()
    {
        RawValue = DefaultValue;
    }

    /// <summary>
    /// Tries to parse the raw value into the specified type.
    /// </summary>
    /// <typeparam name="T">
    /// The type to parse the raw value into.
    /// </typeparam>
    /// <param name="rawValue">
    /// The raw value to parse.
    /// </param>
    /// <param name="result">
    /// The parsed result.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the parsing was successful; otherwise, <see langword="false"/>.
    /// </returns>
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
