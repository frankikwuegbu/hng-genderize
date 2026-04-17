using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Queries
{
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
            var result = await _context.Profiles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (result == null)
            {
                return Result.Failure("no profile exists with that id!");
            }

            return Result.Success(result);
        }
    }
}