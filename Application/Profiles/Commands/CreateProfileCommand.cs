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
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result.Error("Missing or empty parameter", 400);
        }

        var normalizedName = request.Name.Trim().ToLowerInvariant();

        var existingProfile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(profile => profile.Name.ToLower() == normalizedName, cancellationToken);

        if (existingProfile is not null)
        {
            return Result.Success(existingProfile.ToDetailResponse(), "Profile already exists");
        }

        try
        {
            var profile = await _profileServices.CreateProfile(normalizedName, cancellationToken);

            await _context.Profiles.AddAsync(profile, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(profile.ToDetailResponse(), statusCode: 201);
        }
        catch (ExternalApiException exception)
        {
            return Result.Error(exception.Message, 502);
        }
    }
}
