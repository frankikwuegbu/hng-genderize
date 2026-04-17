using System.Text.Json.Serialization;

namespace Application.Common;

public class GenderizeResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("probability")]
    public float? Probability { get; set; }
}
