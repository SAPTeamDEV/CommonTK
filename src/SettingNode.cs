using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAPTeam.CommonTK
{
    public class SettingNode : Node
    {
        private readonly List<Func<string, string, string>> _interceptors = new();

        public SettingNode(SettingNode? parent, string name) : base(parent, name)
        {

        }

        public Setting CreateSetting<T>(string path, T defaultValue, string description = "")
        {
            return CreateSetting(ParsePath(path), defaultValue, description);
        }

        protected Setting CreateSetting<T>(string[] parts, T defaultValue, string description = "")
        {
            if (parts.Length > 1)
            {
                var child = (SettingNode)CreateNode([parts[0]]);
                return child.CreateSetting(parts.Skip(1).ToArray(), defaultValue, description);
            }

            if (!TryGetSetting(parts[0], out Setting setting))
            {
                setting = new Setting<T>(this, parts[0], defaultValue, description);
                AddMember(setting);
            }

            return setting;
        }

        public void AddInterceptor(Func<string, string, string> interceptor)
        {
            _interceptors.Add(interceptor);
        }

        public Setting GetSetting(string path)
        {
            return GetSetting(ParsePath(path));
        }

        protected Setting GetSetting(string[] parts)
        {
            if (parts.Length > 1)
            {
                var child = (SettingNode)GetNode([parts[0]]);
                return child.GetSetting(parts.Skip(1).ToArray());
            }
            if (!TryGetMember(parts[0], out var member))
            {
                throw new KeyNotFoundException($"Setting '{parts[0]}' not found under '{FullPath}'");
            }

            if (member is Setting setting)
            {
                return setting;
            }

            throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting");
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

        public Setting<T> GetSetting<T>(string path)
        {
            return GetSetting<T>(ParsePath(path));
        }

        protected Setting<T> GetSetting<T>(string[] parts)
        {
            if (parts.Length > 1)
            {
                var child = (SettingNode)GetNode([parts[0]]);
                return child.GetSetting<T>(parts.Skip(1).ToArray());
            }
            if (!TryGetMember(parts[0], out var member))
            {
                throw new KeyNotFoundException($"Member '{parts[0]}' not found under '{FullPath}'");
            }

            if (member is Setting<T> setting)
            {
                return setting;
            }

            throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting with type '{typeof(T).Name}'");
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

        protected void UpdateSetting(string[] parts, string value)
        {
            if (parts.Length > 1)
            {
                var child = (SettingNode)GetNode([parts[0]]);
                child.UpdateSetting(parts.Skip(1).ToArray(), value);
                return;
            }

            var member = GetMember([parts[0]]);

            if (member is Setting setting)
            {
                setting.RawValue = value;
            }
            else
            {
                throw new InvalidCastException($"Member '{parts[0]}' under '{FullPath}' is not a setting");
            }
        }

        internal string ApplyInterceptors(string path, string value)
        {
            var current = value;

            foreach (var interceptor in _interceptors)
            {
                current = interceptor(path, current);
            }

            if (Parent is not null and SettingNode settingNode)
            {
                current = settingNode.ApplyInterceptors(string.Join(".", Name, path), current);
            }

            return current;
        }

        public IEnumerable<Setting> GetSettings()
        {
            return GetMembers().OfType<Setting>();
        }

        public IEnumerable<Setting<T>> GetSettings<T>()
        {
            return GetMembers().OfType<Setting<T>>();
        }
    }
}
