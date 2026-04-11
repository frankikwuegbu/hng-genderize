using hng_genderizeApp.Common;
using hng_genderizeApp.Entity;
using Microsoft.AspNetCore.Mvc;

namespace hng_genderizeApp.Services;

public class GenderizeServices
{
    private readonly HttpClient _httpClient;

    public GenderizeServices(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ActionResult<Result>> GetGender(string name)
    {
        //check that name is not null or empty
        if (string.IsNullOrEmpty(name))
        {
            return new BadRequestObjectResult(Result.Error("The 'name' query parameter is required."));
        }
        
        //check that name does not contain a non-letter
        if (ContainsNonString(name))
        {
            return new UnprocessableEntityObjectResult(Result.Error("The 'name' query parameter cannont contain non-letter."));
        }

        var response = await _httpClient.GetFromJsonAsync<GenderizeResponse>($"?name={name}");

        var isConfident = false;

        //confidence calculator
        if (response.Probability >= 0.71 && response.Count >= 100)
        {
            isConfident = true;
        }

        //check is prediction was made
        if (response.Gender == null || response.Count == 0)
        {
            return Result.Error("No prediction available for the provided name");
        }

        var result = new ClassifiedName
        {
            Name = response.Name,
            Gender = response.Gender,
            Probability = response.Probability,
            Sample_Size = response.Count,
            IsConfident = isConfident
        };

        return Result.Success(result);
    }

    public bool ContainsNonString(string name)
    {
        foreach(char character in name)
        {
            if (!char.IsLetter(character))
            { 
                return true;
            }
        }
        return false;
    }
}