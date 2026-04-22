using System.Text.Json;
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

        var existingNames = await _context.Profiles
            .AsNoTracking()
            .Select(profile => profile.Name.ToLower())
            .ToListAsync(cancellationToken);

        var knownNames = existingNames.ToHashSet(StringComparer.Ordinal);
        var profilesToAdd = new List<Profile>();

        foreach (var seedProfile in payload.Profiles)
        {
            if (string.IsNullOrWhiteSpace(seedProfile.Name))
            {
                continue;
            }

            var normalizedName = seedProfile.Name.Trim().ToLowerInvariant();
            if (!knownNames.Add(normalizedName))
            {
                continue;
            }

            profilesToAdd.Add(new Profile
            {
                Id = Guid.CreateVersion7(),
                Name = seedProfile.Name.Trim(),
                Gender = seedProfile.Gender?.Trim().ToLowerInvariant() ?? string.Empty,
                GenderProbability = seedProfile.GenderProbability,
                SampleSize = 0,
                Age = seedProfile.Age,
                AgeGroup = seedProfile.AgeGroup?.Trim().ToLowerInvariant() ?? string.Empty,
                CountryId = seedProfile.CountryId?.Trim().ToUpperInvariant() ?? string.Empty,
                CountryProbability = seedProfile.CountryProbability,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (profilesToAdd.Count == 0)
        {
            _logger.LogInformation("No new seed profiles were inserted because all records already exist.");
            return;
        }

        await _context.Profiles.AddRangeAsync(profilesToAdd, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inserted {Count} seed profiles from {FilePath}.", profilesToAdd.Count, filePath);
    }

    private sealed class SeedProfilesDocument
    {
        public List<SeedProfileItem> Profiles { get; set; } = [];
    }

    private sealed class SeedProfileItem
    {
        public string Name { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public float GenderProbability { get; set; }
        public int Age { get; set; }
        public string AgeGroup { get; set; } = string.Empty;
        public string CountryId { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public float CountryProbability { get; set; }
    }
}
