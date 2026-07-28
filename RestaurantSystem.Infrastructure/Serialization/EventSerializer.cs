using System.Text.Json;

namespace RestaurantSystem.Infrastructure.Serialization;

public sealed class EventSerializer : ISerializer
{
    private readonly JsonSerializerOptions _options;

    public EventSerializer(JsonSerializerOptions options)
    {
        _options = options;
    }

    public string Serialize<T>(T obj)
    {
        // For events, we need the exact type. This needs to be configured carefully
        // to handle polymorphic serialization if necessary, but System.Text.Json
        // often handles this well for concrete event types.
        return JsonSerializer.Serialize(obj, _options);
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, _options)
               ?? throw new JsonException($"Could not deserialize {typeof(T).Name}");
    }

    public object Deserialize(string json, Type type)
    {
        return JsonSerializer.Deserialize(json, type, _options)
               ?? throw new JsonException($"Could not deserialize {type.Name}");
    }
}