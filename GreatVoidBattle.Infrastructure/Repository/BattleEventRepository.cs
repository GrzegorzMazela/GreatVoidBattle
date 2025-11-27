using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Repositories;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for BattleEvent - handles battle event data access.
/// </summary>
public class BattleEventRepository : BaseMongoRepository<BattleEvent, Guid>, IBattleEventRepository
{
    public BattleEventRepository(IMongoDatabase database) 
        : base(database, "BattleEvent")
    {
    }

    protected override Expression<Func<BattleEvent, bool>> GetByIdFilter(Guid id)
    {
        return e => e.EventId == id;
    }

    public async Task AddAsync(BattleEvent battleEvent)
    {
        await CreateAsync(battleEvent);
    }

    public async Task<List<BattleEvent>> GetByBattleIdAsync(Guid battleId)
    {
        return await FindAsync(e => e.BattleId == battleId);
    }
}
