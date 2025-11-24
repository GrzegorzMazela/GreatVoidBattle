using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Repositories;
using MongoDB.Driver;

namespace GreatVoidBattle.Infrastructure.Repository;

public class BattleEventRepository : IBattleEventRepository
{
    private readonly IMongoCollection<BattleEvent> _collection;

    public BattleEventRepository(IMongoDatabase db) =>
       _collection = db.GetCollection<BattleEvent>("BattleEvent");

    public async Task AddAsync(BattleEvent battleEvent) =>
       await _collection.InsertOneAsync(battleEvent);

    public async Task<List<BattleEvent>> GetByBattleIdAsync(Guid battleId)
    {
        var filter = Builders<BattleEvent>.Filter.Eq(e => e.BattleId, battleId);
        var result = await _collection.FindAsync(filter);
        return await result.ToListAsync() ?? [];
    }
}