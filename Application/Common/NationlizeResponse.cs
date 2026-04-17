using System.Text.Json.Serialization;

namespace Application.Common;

public class NationlizeResponse
{
    [JsonPropertyName("count")]
    public int Count { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("country")]
    public List<CountryDto>? Country { get; set; }
}

public class CountryDto
{
    [JsonPropertyName("country_id")]
    public string? Nationality { get; set; }

    [JsonPropertyName("probability")]
    public float Probabilty { get; set; }
}
