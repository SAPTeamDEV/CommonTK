using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAPTeam.CommonTK
{
    /// <summary>
    /// Provides methods for Read/Write Json files.
    /// </summary>
    public abstract class JsonWorker
    {
        /// <summary>
        /// Gets the name of json file.
        /// </summary>
        public string FileName { get; }

        /// <summary>
        /// Gets <see cref="JsonSerializer"/> instance that contains Serializing configurations.
        /// </summary>
        protected JsonSerializer js = new JsonSerializer()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonWorker"/> class.
        /// </summary>
        /// <param name="file">
        /// Name of json file, if the file is not exist it automatically created.
        /// </param>
        public JsonWorker(string file)
        {
            FileName = file;

            if (!File.Exists(file))
            {
                StringWriter sw = new StringWriter();
                JsonTextWriter jw = new JsonTextWriter(sw);
                jw.WriteStartObject();
                jw.WriteEndObject();
                Save(sw);
            }
        }

        /// <summary>
        /// Reads json data from <see cref="FileName"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="JObject"/> created from the Json data of the <see cref="FileName"/>.
        /// </returns>
        protected virtual JObject Open()
        {
            return JObject.Parse(File.ReadAllText(FileName));
        }

        /*
        protected virtual JObject COpen()
        {
            if (!File.Exists(FileName))
            {
                File.Create(FileName);
                return default;
            }
            return Open();
        }
        */

        /// <summary>
        /// Deserializes <paramref name="reader"/> contents into a new instance of <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// A Deserializer class type.
        /// </typeparam>
        /// <param name="reader">
        /// A <see cref="JsonReader"/> object that contains JSON data for deserialization.
        /// </param>
        /// <returns>
        /// A new instance of <typeparamref name="T"/> initialized with <paramref name="reader"/> values.
        /// </returns>
        protected virtual T Parse<T>(JsonReader reader)
            where T : new()
        {
            return js.Deserialize<T>(reader);
        }

        /// <summary>
        /// Serializes <paramref name="deserializerObject"/> values into <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="JsonTextWriter"/> instance for writing serialized strings to it.
        /// </param>
        /// <param name="deserializerObject">
        /// An object that contains some values for converting to json.
        /// </param>
        protected virtual void ToJson(JsonWriter writer, object deserializerObject)
        {
            js.Serialize(writer, deserializerObject);
        }

        /// <summary>
        /// Writes the <see cref="TextWriter"/> contents to <see cref="FileName"/>.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="TextWriter"/> instance that contains json data.
        /// </param>
        protected virtual void Save(TextWriter writer)
        {
            using (StreamWriter file = new StreamWriter(FileName))
            {
                file.WriteLine(writer.ToString());
            }
        }
    }
}