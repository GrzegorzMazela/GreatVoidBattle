using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.GameState;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace GreatVoidBattle.Infrastructure.Repository;

/// <summary>
/// Repository for FractionGameState - handles only data access operations.
/// Business logic is handled in FractionTechnologyService.
/// </summary>
public class FractionGameStateRepository : BaseMongoRepository<FractionGameState, string>, IFractionGameStateRepository
{
    public FractionGameStateRepository(IMongoDatabase database) 
        : base(database, "FractionGameStates")
    {
    }

    protected override Expression<Func<FractionGameState, bool>> GetByIdFilter(string id)
    {
        return s => s.Id == id;
    }

    protected override void OnBeforeUpdate(FractionGameState entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
    }

    public async Task<FractionGameState?> GetByFractionIdAsync(string fractionId)
    {
        return await FindFirstAsync(s => s.FractionId == fractionId);
    }

    public async Task UpdateAsync(FractionGameState state)
    {
        await UpdateAsync(state, state.Id);
    }

    public async Task<List<FractionGameState>> GetAllWithPendingRequestsAsync()
    {
        var allStates = await GetAllAsync();
        return allStates.Where(s => s.ResearchRequests.Any(r => r.Status == ResearchRequestStatus.Pending)).ToList();
    }
}
