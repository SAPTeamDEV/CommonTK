using System;
using System.Collections.Generic;
using System.Text;

namespace SAPTeam.CommonTK
{
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

                if (TryParse<T>(value, out var result))
                {
                    _rawValue = value;
                    UpdateCache(result);
                }

                throw new ArgumentException($"Invalid value '{value}' for setting '{FullPath}'.");
            }
        }

        public T Value
        {
            get
            {
                if (RawValue == _cachedRaw)
                {
                    return _cachedValue;
                }

                if (TryParse<T>(RawValue, out var result))
                {
                    UpdateCache(result);
                    return result;
                }

                throw new InvalidCastException($"Cannot parse setting '{FullPath}' value '{RawValue}' as {typeof(T).Name}.");
            }

            set
            {
                var strValue = value?.ToString() ?? throw new ArgumentNullException("Setting value cannot be null");
                RawValue = strValue;
            }
        }

        private void UpdateCache(T result)
        {
            _cachedValue = result;
            _cachedRaw = RawValue;
        }

        public Setting(SettingNode? parent, string name, T defaultValue, string description = "") : base(parent, name, defaultValue?.ToString() ?? throw new ArgumentNullException(nameof(defaultValue)), description)
        {
            UpdateCache(defaultValue);
        }
    }

    public abstract class Setting : Member
    {
        public virtual string RawValue { get; set; }

        public string DefaultValue { get; }

        public string Description { get; }

        protected Setting(SettingNode? parent, string name, string rawValue, string description = "")
            : base(parent, name)
        {
            DefaultValue = rawValue;
            RawValue = rawValue;
            Description = description;
        }

        static T ChangeType<T>(object obj)
        {
            if (obj is T t)
            {
                return t;
            }

            return (T)obj;
        }

        public T As<T>()
        {
            return TryParse<T>(RawValue, out var result)
                ? result
                : throw new InvalidCastException($"Cannot parse setting '{FullPath}' value '{RawValue}' as {typeof(T).Name}.");
        }

        public static bool TryParse<T>(string rawValue, out T result)
        {
            var type = typeof(T);
            result = default!;

            if (type == typeof(string))
            {
                result = ChangeType<T>(rawValue);
                return true;
            }

            if (type == typeof(bool) && bool.TryParse(rawValue, out var b))
            {
                result = ChangeType<T>(b);
                return true;
            }

            if (type == typeof(int) && int.TryParse(rawValue, out var i))
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
}
