using Application.Common;
using Application.Profiles.Commands;
using Application.Profiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace hng_genderizeApp.Controllers;

[ApiController]
[Route("api/profiles")]
public class ProfilesController : ControllerBase
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllProfiles(
        [FromQuery] string? gender,
        [FromQuery(Name = "country_id")] string? countryId,
        [FromQuery(Name = "age_group")] string? ageGroup,
        [FromQuery(Name = "min_age")] int? minAge,
        [FromQuery(Name = "max_age")] int? maxAge,
        [FromQuery(Name = "min_gender_probability")] float? minGenderProbability,
        [FromQuery(Name = "min_country_probability")] float? minCountryProbability,
        [FromQuery(Name = "sort_by")] string? sortBy,
        [FromQuery] string? order,
        [FromQuery] int? page,
        [FromQuery] int? limit)
    {
        var result = await _sender.Send(new GetAllProfilesQuery
        {
            Gender = gender,
            CountryId = countryId,
            AgeGroup = ageGroup,
            MinAge = minAge,
            MaxAge = maxAge,
            MinGenderProbability = minGenderProbability,
            MinCountryProbability = minCountryProbability,
            SortBy = sortBy,
            Order = order,
            Page = page ?? 1,
            Limit = limit ?? 10
        });

        return ToActionResult(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProfiles(
        [FromQuery(Name = "q")] string? query,
        [FromQuery] int? page,
        [FromQuery] int? limit)
    {
        var result = await _sender.Send(new SearchProfilesQuery
        {
            Q = query,
            Page = page ?? 1,
            Limit = limit ?? 10
        });

        return ToActionResult(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProfileById(Guid id)
    {
        var result = await _sender.Send(new GetProfileByIdQuery
        {
            Id = id
        });

        return ToActionResult(result);
    }

    [HttpGet("{id}")]
    public IActionResult GetProfileByIdInvalid(string id)
    {
        if (Guid.TryParse(id, out _))
        {
            return NotFound();
        }

        return ToActionResult(Result.Error("Invalid type", StatusCodes.Status422UnprocessableEntity));
    }

    [HttpPost]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileCommand command)
    {
        var result = await _sender.Send(command);
        return ToActionResult(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProfile(Guid id)
    {
        var result = await _sender.Send(new DeleteProfileCommand
        {
            Id = id
        });

        return ToActionResult(result);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteProfileInvalid(string id)
    {
        if (Guid.TryParse(id, out _))
        {
            return NotFound();
        }

        return ToActionResult(Result.Error("Invalid type", StatusCodes.Status422UnprocessableEntity));
    }

    private IActionResult ToActionResult(Result result)
    {
        if (result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return StatusCode(result.StatusCode, result);
    }
}
