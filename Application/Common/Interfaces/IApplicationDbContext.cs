using Domain.Entity;
using Microsoft.EntityFrameworkCore;

namespace Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Profile> Profiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
