using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Exploration;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for ExplorationState - handles exploration state data access.
/// </summary>
public class ExplorationStateRepository : BaseMongoRepository<ExplorationState, string>, IExplorationStateRepository
{
    public ExplorationStateRepository(IMongoDatabase database) 
        : base(database, "ExplorationStates")
    {
    }

    protected override Expression<Func<ExplorationState, bool>> GetByIdFilter(string id)
    {
        return s => s.Id == id;
    }

    protected override void OnBeforeUpdate(ExplorationState entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task UpdateAsync(ExplorationState state)
    {
        await UpdateAsync(state, state.Id);
    }

    public async Task<ExplorationState?> GetByFractionIdAsync(string fractionId)
    {
        return await FindFirstAsync(s => s.FractionId == fractionId);
    }

    public async Task<ExplorationState> GetOrCreateByFractionIdAsync(string fractionId)
    {
        var state = await GetByFractionIdAsync(fractionId);
        if (state != null)
            return state;

        state = new ExplorationState
        {
            FractionId = fractionId,
            ResearchSlots = 2 // Default value
        };
        await CreateAsync(state);
        return state;
    }

    public async Task ResetUsedSlotsForAllAsync()
    {
        var filter = Builders<ExplorationState>.Filter.Empty;
        var update = Builders<ExplorationState>.Update
            .Set(s => s.UsedSlotsThisTurn, 0)
            .Set(s => s.UpdatedAt, DateTime.UtcNow);
        
        await _collection.UpdateManyAsync(filter, update);
    }
}

