using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example.ServiceDefaults.Defaults;

public static class JsonSerializerDefaultValues
{
    public static readonly JsonSerializerOptions CacheOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };

    public static readonly JsonSerializerOptions NatsOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
}
