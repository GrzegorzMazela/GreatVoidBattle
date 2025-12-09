using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Exploration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for SystemDiscovery - handles system discovery data access.
/// </summary>
public class SystemDiscoveryRepository : BaseMongoRepository<SystemDiscovery, string>, ISystemDiscoveryRepository
{
    public SystemDiscoveryRepository(IMongoDatabase database) 
        : base(database, "SystemDiscoveries")
    {
    }

    protected override Expression<Func<SystemDiscovery, bool>> GetByIdFilter(string id)
    {
        return d => d.Id == id;
    }

    protected override void OnBeforeUpdate(SystemDiscovery entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateAsync(SystemDiscovery discovery)
    {
        await UpdateAsync(discovery, discovery.Id);
    }

    public async Task<SystemDiscovery?> GetByFractionAndSystemAsync(string fractionId, string systemId)
    {
        return await FindFirstAsync(d => d.FractionId == fractionId && d.SystemId == systemId);
    }

    public async Task<List<SystemDiscovery>> GetByFractionIdAsync(string fractionId)
    {
        return await FindAsync(d => d.FractionId == fractionId);
    }

    public async Task<List<SystemDiscovery>> GetBySystemIdAsync(string systemId)
    {
        return await FindAsync(d => d.SystemId == systemId);
    }

    public async Task<bool> IsSystemKnownByFractionAsync(string fractionId, string systemId)
    {
        var discovery = await GetByFractionAndSystemAsync(fractionId, systemId);
        return discovery != null;
    }
}

