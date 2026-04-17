using System.Text.Json.Serialization;
using Domain.Entity;

namespace Application.Profiles;

public class ProfileDetailResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("gender_probability")]
    public float GenderProbability { get; set; }

    [JsonPropertyName("sample_size")]
    public int SampleSize { get; set; }

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; } = string.Empty;

    [JsonPropertyName("country_id")]
    public string CountryId { get; set; } = string.Empty;

    [JsonPropertyName("country_probability")]
    public float CountryProbability { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }
}

public class ProfileListItemResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("gender")]
    public string Gender { get; set; } = string.Empty;

    [JsonPropertyName("age")]
    public int Age { get; set; }

    [JsonPropertyName("age_group")]
    public string AgeGroup { get; set; } = string.Empty;

    [JsonPropertyName("country_id")]
    public string CountryId { get; set; } = string.Empty;
}

public static class ProfileResponseMapping
{
    public static ProfileDetailResponse ToDetailResponse(this Profile profile)
    {
        return new ProfileDetailResponse
        {
            Id = profile.Id,
            Name = profile.Name,
            Gender = profile.Gender,
            GenderProbability = profile.GenderProbability,
            SampleSize = profile.SampleSize,
            Age = profile.Age,
            AgeGroup = profile.AgeGroup,
            CountryId = profile.CountryId,
            CountryProbability = profile.CountryProbability,
            CreatedAt = profile.CreatedAt
        };
    }

    public static ProfileListItemResponse ToListItemResponse(this Profile profile)
    {
        return new ProfileListItemResponse
        {
            Id = profile.Id,
            Name = profile.Name,
            Gender = profile.Gender,
            Age = profile.Age,
            AgeGroup = profile.AgeGroup,
            CountryId = profile.CountryId
        };
    }
}
