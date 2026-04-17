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
        [FromQuery(Name = "age_group")] string? ageGroup)
    {
        var result = await _sender.Send(new GetAllProfilesQuery
        {
            Gender = gender,
            CountryId = countryId,
            AgeGroup = ageGroup
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

    private IActionResult ToActionResult(Result result)
    {
        if (result.StatusCode == StatusCodes.Status204NoContent)
        {
            return NoContent();
        }

        return StatusCode(result.StatusCode, result);
    }
}
