using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SAPTeam.CommonTK;

public class JsonWorker
{
    public readonly string FileName;

    protected JsonSerializer js = new()
    {
        Formatting = Formatting.Indented
    };

    public JsonWorker(string file)
    {
        FileName = file;
    }

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

    protected virtual T Parse<T>(JsonReader reader)
    {
        return js.Deserialize<T>(reader);
    }

    protected virtual void ToJson(JsonWriter writer, object serializerObject)
    {
        js.Serialize(writer, serializerObject);
    }

    protected virtual void Save(TextWriter writer)
    {
        using StreamWriter file = new(FileName);
        file.WriteLine(writer.ToString());
    }
}
