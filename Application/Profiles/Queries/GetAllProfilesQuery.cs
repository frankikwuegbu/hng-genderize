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
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public float? MinGenderProbability { get; set; }
    public float? MinCountryProbability { get; set; }
    public string? SortBy { get; set; }
    public string? Order { get; set; }
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 10;
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
        var page = request.Page < 1 ? 1 : request.Page;
        var limit = request.Limit < 1 ? 10 : Math.Min(request.Limit, 50);

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

        if (request.MinAge.HasValue)
        {
            query = query.Where(profile => profile.Age >= request.MinAge.Value);
        }

        if (request.MaxAge.HasValue)
        {
            query = query.Where(profile => profile.Age <= request.MaxAge.Value);
        }

        if (request.MinGenderProbability.HasValue)
        {
            query = query.Where(profile => profile.GenderProbability >= request.MinGenderProbability.Value);
        }

        if (request.MinCountryProbability.HasValue)
        {
            query = query.Where(profile => profile.CountryProbability >= request.MinCountryProbability.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var normalizedSortBy = request.SortBy?.Trim().ToLowerInvariant();
        var normalizedOrder = request.Order?.Trim().ToLowerInvariant();
        var isDescending = normalizedOrder == "desc";

        query = (normalizedSortBy, isDescending) switch
        {
            ("age", true) => query.OrderByDescending(profile => profile.Age).ThenByDescending(profile => profile.CreatedAt),
            ("age", false) => query.OrderBy(profile => profile.Age).ThenBy(profile => profile.CreatedAt),
            ("gender_probability", true) => query.OrderByDescending(profile => profile.GenderProbability).ThenByDescending(profile => profile.CreatedAt),
            ("gender_probability", false) => query.OrderBy(profile => profile.GenderProbability).ThenBy(profile => profile.CreatedAt),
            (_, true) => query.OrderByDescending(profile => profile.CreatedAt).ThenByDescending(profile => profile.Id),
            _ => query.OrderBy(profile => profile.CreatedAt).ThenBy(profile => profile.Id)
        };

        var profiles = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var response = profiles
            .Select(profile => profile.ToListItemResponse())
            .ToList();

        return Result.Success(response, page: page, limit: limit, total: total);
    }
}
