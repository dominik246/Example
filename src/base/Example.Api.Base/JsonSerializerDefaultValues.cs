using System.Text.Json;
using System.Text.Json.Serialization;

namespace Example.Api.Base;

public static class JsonSerializerDefaultValues
{
    public static readonly JsonSerializerOptions CacheOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault
    };
}
