using GreatVoidBattle.Core.Domains.Exploration;

namespace GreatVoidBattle.Application.Repositories;

public interface IStarSystemRepository
{
    Task<StarSystem?> GetByIdAsync(string id);
    Task<List<StarSystem>> GetAllAsync();
    Task<StarSystem> CreateAsync(StarSystem system);
    Task UpdateAsync(StarSystem system);
    Task DeleteAsync(string id);
    Task<List<StarSystem>> GetByIdsAsync(List<string> ids);
    Task<List<StarSystem>> GetConnectedSystemsAsync(string systemId);
}

