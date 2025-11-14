using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains;
using MongoDB.Driver;

namespace GreatVoidBattle.Infrastructure.Repository;

public class BattleStateRepository : IBattleStateRepository
{
    private readonly IMongoCollection<BattleState> _collection;

    public BattleStateRepository(IMongoDatabase db) =>
        _collection = db.GetCollection<BattleState>("BattleState");

    public async Task AddAsync(BattleState battleState) =>
        await _collection.InsertOneAsync(battleState);

    public async Task UpdateAsync(Guid id, BattleState battleState) =>
    await _collection.ReplaceOneAsync(x => x.BattleId == id, battleState);

    public async Task<BattleState?> GetByIdAsync(Guid id) =>
        await _collection.Find(x => x.BattleId == id).FirstOrDefaultAsync();

    public async Task<IEnumerable<BattleDto>> GetBattles()
    {
        var battles = await _collection.Find(_ => true).ToListAsync();
        return battles.Select(b => new BattleDto(
            b.BattleId,
            b.BattleName,
            b.CreatedAt,
            b.BattleStatus.ToString(),
            b.TurnNumber,
            b.Width,
            b.Height,
            b.Fractions.Select(f => f.FractionName).ToList()
        ));
    }
}