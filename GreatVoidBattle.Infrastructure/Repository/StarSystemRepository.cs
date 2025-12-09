using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Exploration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for StarSystem - handles star system data access.
/// </summary>
public class StarSystemRepository : BaseMongoRepository<StarSystem, string>, IStarSystemRepository
{
    public StarSystemRepository(IMongoDatabase database) 
        : base(database, "StarSystems")
    {
    }

    protected override Expression<Func<StarSystem, bool>> GetByIdFilter(string id)
    {
        return s => s.Id == id;
    }

    protected override void OnBeforeUpdate(StarSystem entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateAsync(StarSystem system)
    {
        await UpdateAsync(system, system.Id);
    }

    public async Task<List<StarSystem>> GetByIdsAsync(List<string> ids)
    {
        return await FindAsync(s => ids.Contains(s.Id));
    }

    public async Task<List<StarSystem>> GetConnectedSystemsAsync(string systemId)
    {
        var system = await GetByIdAsync(systemId);
        if (system == null || !system.ConnectedSystems.Any())
            return new List<StarSystem>();

        return await GetByIdsAsync(system.ConnectedSystems);
    }
}

