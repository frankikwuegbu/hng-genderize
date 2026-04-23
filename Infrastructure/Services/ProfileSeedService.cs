using System.Text.Json;
using System.Text.Json.Serialization;
using Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class ProfileSeedService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProfileSeedService> _logger;

    public ProfileSeedService(ApplicationDbContext context, ILogger<ProfileSeedService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("Seed file was not found at {FilePath}. Skipping profile seed.", filePath);
            return;
        }

        await using var stream = File.OpenRead(filePath);
        var payload = await JsonSerializer.DeserializeAsync<SeedProfilesDocument>(
            stream,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            },
            cancellationToken);

        if (payload?.Profiles is null || payload.Profiles.Count == 0)
        {
            _logger.LogInformation("Seed file {FilePath} contained no profiles. Nothing to import.", filePath);
            return;
        }

        var existingProfiles = await _context.Profiles
            .ToListAsync(cancellationToken);

        var existingProfilesByName = existingProfiles.ToDictionary(
            profile => profile.Name.Trim().ToLowerInvariant(),
            StringComparer.Ordinal);
        var profilesToAdd = new List<Profile>();
        var updatedCount = 0;

        foreach (var seedProfile in payload.Profiles)
        {
            if (string.IsNullOrWhiteSpace(seedProfile.Name))
            {
                continue;
            }

            var normalizedName = seedProfile.Name.Trim().ToLowerInvariant();
            var countryName = string.IsNullOrWhiteSpace(seedProfile.CountryName)
                ? GetCountryName(seedProfile.CountryId)
                : seedProfile.CountryName.Trim();

            if (existingProfilesByName.TryGetValue(normalizedName, out var existingProfile))
            {
                existingProfile.Name = seedProfile.Name.Trim();
                existingProfile.Gender = seedProfile.Gender?.Trim().ToLowerInvariant() ?? string.Empty;
                existingProfile.GenderProbability = seedProfile.GenderProbability;
                existingProfile.Age = seedProfile.Age;
                existingProfile.AgeGroup = seedProfile.AgeGroup?.Trim().ToLowerInvariant() ?? string.Empty;
                existingProfile.CountryId = seedProfile.CountryId?.Trim().ToUpperInvariant() ?? string.Empty;
                existingProfile.CountryName = countryName;
                existingProfile.CountryProbability = seedProfile.CountryProbability;
                updatedCount++;
                continue;
            }

            var profile = new Profile
            {
                Id = Guid.CreateVersion7(),
                Name = seedProfile.Name.Trim(),
                Gender = seedProfile.Gender?.Trim().ToLowerInvariant() ?? string.Empty,
                GenderProbability = seedProfile.GenderProbability,
                SampleSize = 0,
                Age = seedProfile.Age,
                AgeGroup = seedProfile.AgeGroup?.Trim().ToLowerInvariant() ?? string.Empty,
                CountryId = seedProfile.CountryId?.Trim().ToUpperInvariant() ?? string.Empty,
                CountryName = countryName,
                CountryProbability = seedProfile.CountryProbability,
                CreatedAt = DateTime.UtcNow
            };

            profilesToAdd.Add(profile);
            existingProfilesByName[normalizedName] = profile;
        }

        if (profilesToAdd.Count == 0 && updatedCount == 0)
        {
            _logger.LogInformation("Seed file matched the existing profile data. Nothing to import.");
            return;
        }

        if (profilesToAdd.Count > 0)
        {
            await _context.Profiles.AddRangeAsync(profilesToAdd, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Applied seed data from {FilePath}. Inserted {InsertedCount} profiles and updated {UpdatedCount} profiles.",
            filePath,
            profilesToAdd.Count,
            updatedCount);
    }

    private sealed class SeedProfilesDocument
    {
        [JsonPropertyName("profiles")]
        public List<SeedProfileItem> Profiles { get; set; } = [];
    }

    private sealed class SeedProfileItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("gender")]
        public string Gender { get; set; } = string.Empty;

        [JsonPropertyName("gender_probability")]
        public float GenderProbability { get; set; }

        [JsonPropertyName("age")]
        public int Age { get; set; }

        [JsonPropertyName("age_group")]
        public string AgeGroup { get; set; } = string.Empty;

        [JsonPropertyName("country_id")]
        public string CountryId { get; set; } = string.Empty;

        [JsonPropertyName("country_name")]
        public string CountryName { get; set; } = string.Empty;

        [JsonPropertyName("country_probability")]
        public float CountryProbability { get; set; }
    }

    private static string GetCountryName(string? countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return string.Empty;
        }

        try
        {
            return new System.Globalization.RegionInfo(countryId.Trim().ToUpperInvariant()).EnglishName;
        }
        catch (ArgumentException)
        {
            return countryId.Trim().ToUpperInvariant();
        }
    }
}
