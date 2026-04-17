using Domain.Entity;

namespace Application.Common.Interfaces;

public interface IProfileServices
{
    Task<Profile> CreateProfile(string name);
    Task<AgeifyResponse?> GetAgeAndAgeGroup(string name);
    Task<CountryDto?> GetCountry(string name);
}
