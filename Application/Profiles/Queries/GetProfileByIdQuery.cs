using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Queries;

public class GetProfileByIdQuery : IRequest<Result>
{
    public Guid Id { get; set; }
}

public class GetProfileByIdQueryHandler : IRequestHandler<GetProfileByIdQuery, Result>
{
    private readonly IApplicationDbContext _context;

    public GetProfileByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(GetProfileByIdQuery request, CancellationToken cancellationToken)
    {
        var profile = await _context.Profiles
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Error("Profile not found", 404);
        }

        return Result.Success(profile.ToDetailResponse());
    }
}
