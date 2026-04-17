using Domain.Entity;

namespace Application.Common.Interfaces;

public interface IProfileServices
{
    Task<Profile> CreateProfile(string name, CancellationToken cancellationToken = default);
}
