using GreatVoidBattle.Core.Domains.Exploration;

namespace GreatVoidBattle.Application.Repositories;

public interface IExplorationStateRepository
{
    Task<ExplorationState?> GetByIdAsync(string id);
    Task<List<ExplorationState>> GetAllAsync();
    Task<ExplorationState> CreateAsync(ExplorationState state);
    Task UpdateAsync(ExplorationState state);
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Pobiera stan eksploracji dla frakcji
    /// </summary>
    Task<ExplorationState?> GetByFractionIdAsync(string fractionId);
    
    /// <summary>
    /// Pobiera lub tworzy stan eksploracji dla frakcji
    /// </summary>
    Task<ExplorationState> GetOrCreateByFractionIdAsync(string fractionId);
    
    /// <summary>
    /// Resetuje wykorzystane sloty dla wszystkich frakcji (koniec tury)
    /// </summary>
    Task ResetUsedSlotsForAllAsync();
}

