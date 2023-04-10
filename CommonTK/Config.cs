using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides methods for create, use, change and save Application configuration files.
    /// </summary>
    /// <typeparam name="T">
    /// A Deserializer class type that determines the configuration JSON file structure.
    /// </typeparam>
    public sealed class Config<T>
        where T : new()
    {
        JsonSerializer js = new JsonSerializer()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Gets the name of JSON file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> that contains the configuration values.
        /// </summary>
        public T Prefs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config{T}"/> class.
        /// </summary>
        /// <param name="configPath">
        /// Path of configuration file, if the file does not exists, it automatically created.
        /// </param>
        public Config(string configPath)
        {
            FileName = configPath;

            if (!File.Exists(configPath))
            {
                StringWriter sw = new StringWriter();
                JsonTextWriter jw = new JsonTextWriter(sw);
                jw.WriteStartObject();
                jw.WriteEndObject();
                Commit(sw);
            }

            JObject data = JObject.Parse(File.ReadAllText(configPath));

            Prefs = js.Deserialize<T>(data.CreateReader());
        }

        /// <summary>
        /// Saves current values of <see cref="Prefs"/> properties to the configuration file.
        /// <para>
        /// Method Action Group: application.config.<see href="FileName"/>
        /// </para>
        /// </summary>
        public void Write()
        {
            Context.QueryGroup(Context.ActionGroup(ActionScope.Application, "config", FileName));
            StringWriter sw = new StringWriter();
            JsonTextWriter jw = new JsonTextWriter(sw);
            js.Serialize(jw, Prefs);
            Commit(sw);
        }

        private void Commit(StringWriter writer)
        {
            using (StreamWriter file = new StreamWriter(FileName))
            {
                file.WriteLine(writer.ToString());
            }
        }
    }
}