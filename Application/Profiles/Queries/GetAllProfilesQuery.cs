using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Queries;

public class GetAllProfilesQuery : IRequest<Result>
{
    public string? Gender { get; set; }
    public string? CountryId { get; set; }
    public string? AgeGroup { get; set; }
}

public class GetAllProfileQueryHandler : IRequestHandler<GetAllProfilesQuery, Result>
{
    private readonly IApplicationDbContext _context;

    public GetAllProfileQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(GetAllProfilesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Profiles.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            var normalizedGender = request.Gender.Trim().ToLowerInvariant();
            query = query.Where(profile => profile.Gender.ToLower() == normalizedGender);
        }

        if (!string.IsNullOrWhiteSpace(request.CountryId))
        {
            var normalizedCountryId = request.CountryId.Trim().ToLowerInvariant();
            query = query.Where(profile => profile.CountryId.ToLower() == normalizedCountryId);
        }

        if (!string.IsNullOrWhiteSpace(request.AgeGroup))
        {
            var normalizedAgeGroup = request.AgeGroup.Trim().ToLowerInvariant();
            query = query.Where(profile => profile.AgeGroup.ToLower() == normalizedAgeGroup);
        }

        var profiles = await query
            .OrderBy(profile => profile.CreatedAt)
            .ToListAsync(cancellationToken);

        var response = profiles
            .Select(profile => profile.ToListItemResponse())
            .ToList();

        return Result.Success(response, count: response.Count);
    }
}
