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
    public class Config<T> : JsonWorker
        where T : new()
    {
        /// <summary>
        /// Gets an instance of <typeparamref name="T"/> that contains configuration values.
        /// </summary>
        public T Prefs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Config{T}"/> class.
        /// </summary>
        /// <param name="configPath">
        /// Path of configuration file, if the file is not exist it automatically created.
        /// </param>
        public Config(string configPath) : base(configPath) => Prefs = Parse<T>(Open().CreateReader());

        /// <summary>
        /// Saves current values of <see cref="Prefs"/> properties to the configuration file.
        /// </summary>
        public void Write() => Write(Prefs);

        private void Write(T cObject)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jw = new JsonTextWriter(sw);
            ToJson(jw, cObject);
            Save(sw);
        }
    }
}