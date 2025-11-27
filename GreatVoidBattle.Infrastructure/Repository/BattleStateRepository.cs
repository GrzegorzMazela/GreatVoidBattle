using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for BattleState - handles battle state data access.
/// </summary>
public class BattleStateRepository : BaseMongoRepository<BattleState, Guid>, IBattleStateRepository
{
    public BattleStateRepository(IMongoDatabase database) 
        : base(database, "BattleState")
    {
    }

    protected override Expression<Func<BattleState, bool>> GetByIdFilter(Guid id)
    {
        return x => x.BattleId == id;
    }

    protected override void OnBeforeUpdate(BattleState entity)
    {
        entity.LastUpdated = DateTime.UtcNow;
    }

    public async Task AddAsync(BattleState battleState)
    {
        await CreateAsync(battleState);
    }

    public async Task UpdateAsync(Guid id, BattleState battleState)
    {
        await UpdateAsync(battleState, id);
    }

    public new async Task<BattleState?> GetByIdAsync(Guid id)
    {
        return await FindFirstAsync(x => x.BattleId == id && !x.IsDeleted);
    }

    public async Task<IEnumerable<BattleDto>> GetBattles()
    {
        var battles = await FindAsync(b => !b.IsDeleted);
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

    public async Task<bool> SoftDeleteAsync(Guid id)
    {
        var update = Builders<BattleState>.Update
            .Set(b => b.IsDeleted, true)
            .Set(b => b.LastUpdated, DateTime.UtcNow);
        
        var result = await _collection.UpdateOneAsync(GetByIdFilter(id), update);
        return result.ModifiedCount > 0;
    }
}
