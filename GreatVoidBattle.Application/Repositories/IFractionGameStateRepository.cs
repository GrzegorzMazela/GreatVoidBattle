using GreatVoidBattle.Core.Domains.GameState;

namespace GreatVoidBattle.Application.Repositories;

/// <summary>
/// Repository interface for FractionGameState - handles only data access operations.
/// Business logic should be in the service layer.
/// </summary>
public interface IFractionGameStateRepository
{
    Task<FractionGameState?> GetByFractionIdAsync(string fractionId);

    Task<FractionGameState?> GetByIdAsync(string id);

    Task<List<FractionGameState>> GetAllAsync();

    Task<FractionGameState> CreateAsync(FractionGameState state);

    Task UpdateAsync(FractionGameState state);

    Task DeleteAsync(string id);

    /// <summary>
    /// Gets all states that have pending research requests
    /// </summary>
    Task<List<FractionGameState>> GetAllWithPendingRequestsAsync();
}
