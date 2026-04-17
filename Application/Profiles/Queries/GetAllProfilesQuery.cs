using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Queries;

public class GetAllProfilesQuery : IRequest<Result>
{

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
        var result = await _context.Profiles.AsNoTracking().ToListAsync(cancellationToken);

        return Result.Success(result, result.Count);
    }
}