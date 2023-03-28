using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAPTeam.CommonTK
{
    public class Config<T> : JsonWorker
        where T : new()
    {
        public T Prefs { get; }

        public Config(string configPath) : base(configPath)
        {
            if (!File.Exists(FileName))
            {
                Write(new T());
            }
            JObject cData = Open();
            Prefs = Parse<T>(cData.CreateReader());
        }

        public void Write()
        {
            Write(Prefs);
        }

        private void Write(T cObject)
        {
            StringWriter sw = new();
            JsonTextWriter jw = new(sw);
            ToJson(jw, cObject);
            Save(sw);
        }
    }
}