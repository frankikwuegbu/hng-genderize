using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Commands;

public class CreateProfileCommand : IRequest<Result>
{
    public string Name { get; set; } = string.Empty;
}

public class CreateProfileCommandHandler : IRequestHandler<CreateProfileCommand, Result>
{
    private readonly IProfileServices _profileServices;
    private readonly IApplicationDbContext _context;

    public CreateProfileCommandHandler(IProfileServices profileServices, IApplicationDbContext context)
    {
        _profileServices = profileServices;
        _context = context;

    }

    public async Task<Result> Handle(CreateProfileCommand request, CancellationToken cancellationToken)
    {
        //create profile
        var result = await _profileServices.CreateProfile(request.Name);

        if (string.IsNullOrWhiteSpace(result.Name))
        {
            return Result.Failure("a profile could not be created for the name provided");
        }

        //check if profile exists
        var existingProfile = await _context.Profiles.FirstOrDefaultAsync(x => x.Name == request.Name, cancellationToken);

        if (existingProfile is not null)
        {
            return Result.Failure("this profile already exists", existingProfile);
        }

        await _context.Profiles.AddAsync(result, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(result);
    }
}
