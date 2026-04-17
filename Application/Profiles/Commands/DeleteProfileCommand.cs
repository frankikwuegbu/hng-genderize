using Application.Common;
using Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles.Commands;

public class DeleteProfileCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}

public class DeleteProfileCommandHandler : IRequestHandler<DeleteProfileCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteProfileCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var result = await _context.Profiles.FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (result is null)
        {
            return Result.Failure("the profile does not exist");
        }

        _context.Profiles.Remove(result);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success("profile deleted");
    }
}