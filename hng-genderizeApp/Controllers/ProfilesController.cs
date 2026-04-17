using Application.Common;
using Application.Profiles.Commands;
using Application.Profiles.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace hng_genderizeApp.Controllers;

[ApiController]
[Route("api")]
public class ProfilesController : ControllerBase
{
    private readonly ISender _sender;

    public ProfilesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("profiles")]
    public async Task<ActionResult<Result>> GetAllProfiles()
    {
        return await _sender.Send(new GetAllProfilesQuery());
    }

    [HttpGet("profiles/{id:guid}")]
    public async Task<ActionResult<Result>> GetProfileById(Guid id)
    {
        return await _sender.Send(new GetProfileByIdQuery
        {
            Id = id
        });
    }

    [HttpPost("profiles")]
    public async Task<ActionResult<Result>> CreateProfile([FromBody] CreateProfileCommand command)
    {
        return await _sender.Send(command);
    }
}
