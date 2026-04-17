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
        var profile = await _context.Profiles.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (profile is null)
        {
            return Result.Error("Profile not found", 404);
        }

        _context.Profiles.Remove(profile);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.NoContent();
    }
}
