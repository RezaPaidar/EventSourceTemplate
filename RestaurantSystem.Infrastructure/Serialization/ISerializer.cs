namespace RestaurantSystem.Infrastructure.Serialization;

public interface ISerializer
{
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
    object Deserialize(string json, Type type);
}