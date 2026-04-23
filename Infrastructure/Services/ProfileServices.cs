using System.Net.Http.Json;
using System.Globalization;
using Application.Common;
using Application.Common.Interfaces;
using Domain.Entity;

namespace Infrastructure.Services;

public class ProfileServices : IProfileServices
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ProfileServices(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<Profile> CreateProfile(string name, CancellationToken cancellationToken = default)
    {
        var genderize = await GetGenderize(name, cancellationToken);
        var agify = await GetAgify(name, cancellationToken);
        var nationalize = await GetNationalize(name, cancellationToken);

        return new Profile
        {
            Id = Guid.CreateVersion7(),
            Name = name,
            Gender = genderize.Gender!,
            GenderProbability = genderize.Probability!.Value,
            SampleSize = genderize.Count!.Value,
            Age = agify.Age!.Value,
            AgeGroup = GetAgeGroup(agify.Age.Value),
            CountryId = nationalize.CountryId!,
            CountryName = GetCountryName(nationalize.CountryId),
            CountryProbability = nationalize.Probability!.Value,
            CreatedAt = DateTime.UtcNow
        };
    }

    private async Task<GenderizeResponse> GetGenderize(string name, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("genderize");

        try
        {
            var response = await client.GetFromJsonAsync<GenderizeResponse>($"?name={name}", cancellationToken);

            if (response?.Gender == null || response.Count is null || response.Count == 0 || response.Probability is null)
            {
                throw new ExternalApiException("Genderize");
            }

            return response;
        }
        catch (HttpRequestException)
        {
            throw new ExternalApiException("Genderize");
        }
        catch (NotSupportedException)
        {
            throw new ExternalApiException("Genderize");
        }
        catch (ExternalApiException)
        {
            throw;
        }
        catch
        {
            throw new ExternalApiException("Genderize");
        }
    }

    private async Task<AgeifyResponse> GetAgify(string name, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("agify");

        try
        {
            var response = await client.GetFromJsonAsync<AgeifyResponse>($"?name={name}", cancellationToken);

            if (response?.Age is null)
            {
                throw new ExternalApiException("Agify");
            }

            return response;
        }
        catch (HttpRequestException)
        {
            throw new ExternalApiException("Agify");
        }
        catch (NotSupportedException)
        {
            throw new ExternalApiException("Agify");
        }
        catch (ExternalApiException)
        {
            throw;
        }
        catch
        {
            throw new ExternalApiException("Agify");
        }
    }

    private async Task<CountryDto> GetNationalize(string name, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("nationalize");

        try
        {
            var response = await client.GetFromJsonAsync<NationlizeResponse>($"?name={name}", cancellationToken);

            var country = response?.Country?
                .Where(item => !string.IsNullOrWhiteSpace(item.CountryId) && item.Probability is not null)
                .OrderByDescending(item => item.Probability)
                .FirstOrDefault();

            if (country == null)
            {
                throw new ExternalApiException("Nationalize");
            }

            return country;
        }
        catch (HttpRequestException)
        {
            throw new ExternalApiException("Nationalize");
        }
        catch (NotSupportedException)
        {
            throw new ExternalApiException("Nationalize");
        }
        catch (ExternalApiException)
        {
            throw;
        }
        catch
        {
            throw new ExternalApiException("Nationalize");
        }
    }

    private static string GetAgeGroup(int age)
    {
        if (age <= 12)
        {
            return "child";
        }

        if (age <= 19)
        {
            return "teenager";
        }

        if (age <= 59)
        {
            return "adult";
        }

        return "senior";
    }

    private static string GetCountryName(string? countryId)
    {
        if (string.IsNullOrWhiteSpace(countryId))
        {
            return string.Empty;
        }

        try
        {
            return new RegionInfo(countryId.Trim().ToUpperInvariant()).EnglishName;
        }
        catch (ArgumentException)
        {
            return countryId.Trim().ToUpperInvariant();
        }
    }
}
