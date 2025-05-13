using System.Text.Json;
using System.Text.Json.Serialization;

namespace SAPTeam.CommonTK;

/// <summary>
/// Represents methods to work with json serialization and deserialization with full support for AOT.
/// </summary>
public class JsonWorker
{
    private readonly JsonSerializerOptions options;
    private readonly JsonSerializerContext context;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWorker"/> class.
    /// </summary>
    /// <param name="options">
    /// The serializer options. (used in netstandard2.0 targets)
    /// </param>
    /// <param name="context">
    /// The source-generated serializer context. (used in net6.0+ targets)
    /// </param>
    public JsonWorker(JsonSerializerOptions options = null, JsonSerializerContext context = null)
    {
        this.options = options;
        this.context = context;
    }

#if NET6_0_OR_GREATER
    /// <summary>
    /// Converts the provided object to json string.
    /// </summary>
    /// <param name="obj">
    /// The object to convert.
    /// </param>
    /// <returns>
    /// The converted json string.
    /// </returns>
    public string ToJson(object obj) => JsonSerializer.Serialize(obj, obj.GetType(), context);

    /// <summary>
    /// Converts the provided json string to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">
    /// The json string to convert.
    /// </param>
    /// <typeparam name="T">
    /// The type of the object to convert to and return.
    /// </typeparam>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> initialized with json data.
    /// </returns>
    public T ReadJson<T>(string json)
        where T : class => JsonSerializer.Deserialize(json, typeof(T), context) as T;
#else
    /// <summary>
    /// Converts the provided object to json string.
    /// </summary>
    /// <param name="obj">
    /// The object to convert.
    /// </param>
    /// <returns>
    /// The converted json string.
    /// </returns>
    public string ToJson(object obj) => JsonSerializer.Serialize(obj, obj.GetType(), options);

    /// <summary>
    /// Converts the provided json string to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">
    /// The json string to convert.
    /// </param>
    /// <typeparam name="T">
    /// The type of the object to convert to and return.
    /// </typeparam>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> initialized with json data.
    /// </returns>
    public T ReadJson<T>(string json)
        where T : class => JsonSerializer.Deserialize(json, typeof(T), options) as T;
#endif

#if NET6_0_OR_GREATER
    /// <summary>
    /// Converts the provided object to json string.
    /// </summary>
    /// <remarks>
    /// This method is AOT-Compliant and you must prepare your types with System.Text.Json source generation.
    /// </remarks>
    /// <param name="obj">
    /// The object to convert.
    /// </param>
    /// <param name="context">
    /// A metadata provider for serializable types.
    /// </param>
    /// <returns>
    /// The converted json string.
    /// </returns>
    public static string ToJson(object obj, JsonSerializerContext context) => JsonSerializer.Serialize(obj, obj.GetType(), context);

    /// <summary>
    /// Converts the provided json string to <typeparamref name="T"/>.
    /// </summary>
    /// <remarks>
    /// This method is AOT-Compliant and you must prepare your types with System.Text.Json source generation.
    /// </remarks>
    /// <param name="json">
    /// The json string to convert.
    /// </param>
    /// <param name="context">
    /// A metadata provider for serializable types.
    /// </param>
    /// <typeparam name="T">
    /// The type of the object to convert to and return.
    /// </typeparam>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> initialized with json data.
    /// </returns>
    public static T ReadJson<T>(string json, JsonSerializerContext context)
        where T : class => JsonSerializer.Deserialize(json, typeof(T), context) as T;
#else
    /// <summary>
    /// Converts the provided object to json string.
    /// </summary>
    /// <param name="obj">
    /// The object to convert.
    /// </param>
    /// <param name="options">
    /// Options to control the conversion behavior.
    /// </param>
    /// <returns>
    /// The converted json string.
    /// </returns>
    public static string ToJson(object obj, JsonSerializerOptions options) => JsonSerializer.Serialize(obj, obj.GetType(), options);

    /// <summary>
    /// Converts the provided json string to <typeparamref name="T"/>.
    /// </summary>
    /// <param name="json">
    /// The json string to convert.
    /// </param>
    /// <param name="options">
    /// Options to control the conversion behavior.
    /// </param>
    /// <typeparam name="T">
    /// The type of the object to convert to and return.
    /// </typeparam>
    /// <returns>
    /// A new instance of <typeparamref name="T"/> initialized with json data.
    /// </returns>
    public static T ReadJson<T>(string json, JsonSerializerOptions options)
        where T : class => JsonSerializer.Deserialize(json, typeof(T), options) as T;
#endif
}
