using System.Net.Http.Json;
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

    public async Task<Profile> CreateProfile(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || ContainsNonLetter(name))
        {
            return new Profile();
        }

        var genderClient = _httpClientFactory.CreateClient("genderize");
        var ageAndGroup = await GetAgeAndAgeGroup(name);
        var country = await GetCountry(name);
        var genderResponse = await genderClient.GetFromJsonAsync<GenderizeResponse>($"?name={name}");

        if (genderResponse == null || country == null || ageAndGroup == null)
        {
            return new Profile();
        }

        return new Profile
        {
            Name = name,
            Gender = genderResponse.Gender,
            GenderProbability = genderResponse.Probability,
            Sample_Size = genderResponse.Count,
            Age = ageAndGroup.Age,
            AgeGroup = ageAndGroup.AgeGroup,
            CountryId = country.Nationality,
            CountryProbability = country.Probabilty
        };
    }

    public async Task<AgeifyResponse?> GetAgeAndAgeGroup(string name)
    {
        var ageClient = _httpClientFactory.CreateClient("agify");
        var ageResponse = await ageClient.GetFromJsonAsync<AgeifyResponse>($"?name={name}");

        if (ageResponse == null)
        {
            return null;
        }

        if (ageResponse.Age <= 12)
        {
            ageResponse.AgeGroup = "child";
        }
        else if (ageResponse.Age <= 19)
        {
            ageResponse.AgeGroup = "teenager";
        }
        else if (ageResponse.Age <= 59)
        {
            ageResponse.AgeGroup = "adult";
        }
        else
        {
            ageResponse.AgeGroup = "elder";
        }

        return ageResponse;
    }

    public async Task<CountryDto?> GetCountry(string name)
    {
        var nationalityClient = _httpClientFactory.CreateClient("nationalize");
        var nationalityResponse = await nationalityClient.GetFromJsonAsync<NationlizeResponse>($"?name={name}");

        if (nationalityResponse?.Country == null)
        {
            return null;
        }

        return nationalityResponse.Country
            .OrderByDescending(country => country.Probabilty)
            .FirstOrDefault();
    }

    private static bool ContainsNonLetter(string value)
    {
        return value.Any(character => !char.IsLetter(character));
    }
}
