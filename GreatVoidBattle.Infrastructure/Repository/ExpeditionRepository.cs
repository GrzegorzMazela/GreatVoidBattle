using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Exploration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for Expedition - handles expedition data access.
/// </summary>
public class ExpeditionRepository : BaseMongoRepository<Expedition, string>, IExpeditionRepository
{
    public ExpeditionRepository(IMongoDatabase database) 
        : base(database, "Expeditions")
    {
    }

    protected override Expression<Func<Expedition, bool>> GetByIdFilter(string id)
    {
        return e => e.Id == id;
    }

    protected override void OnBeforeUpdate(Expedition entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateAsync(Expedition expedition)
    {
        await UpdateAsync(expedition, expedition.Id);
    }

    public async Task<List<Expedition>> GetByFractionIdAsync(string fractionId)
    {
        return await FindAsync(e => e.FractionId == fractionId);
    }

    public async Task<List<Expedition>> GetPendingByFractionIdAsync(string fractionId)
    {
        return await FindAsync(e => e.FractionId == fractionId && e.Status == ExpeditionStatus.Pending);
    }

    public async Task<List<Expedition>> GetAllPendingAsync()
    {
        return await FindAsync(e => e.Status == ExpeditionStatus.Pending);
    }

    public async Task<List<Expedition>> GetBySystemIdAsync(string systemId)
    {
        return await FindAsync(e => e.SystemId == systemId);
    }

    public async Task<List<Expedition>> GetHistoryByFractionIdAsync(string fractionId)
    {
        return await FindAsync(e => 
            e.FractionId == fractionId && 
            e.Status != ExpeditionStatus.Pending);
    }
}

