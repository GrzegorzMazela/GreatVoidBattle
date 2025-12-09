using GreatVoidBattle.Core.Domains.Exploration;

namespace GreatVoidBattle.Application.Repositories;

public interface IExpeditionRepository
{
    Task<Expedition?> GetByIdAsync(string id);
    Task<List<Expedition>> GetAllAsync();
    Task<Expedition> CreateAsync(Expedition expedition);
    Task UpdateAsync(Expedition expedition);
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Pobiera ekspedycje frakcji
    /// </summary>
    Task<List<Expedition>> GetByFractionIdAsync(string fractionId);
    
    /// <summary>
    /// Pobiera oczekujące ekspedycje frakcji
    /// </summary>
    Task<List<Expedition>> GetPendingByFractionIdAsync(string fractionId);
    
    /// <summary>
    /// Pobiera wszystkie oczekujące ekspedycje (dla admina)
    /// </summary>
    Task<List<Expedition>> GetAllPendingAsync();
    
    /// <summary>
    /// Pobiera ekspedycje do danego układu
    /// </summary>
    Task<List<Expedition>> GetBySystemIdAsync(string systemId);
    
    /// <summary>
    /// Pobiera historię ekspedycji frakcji (zakończone)
    /// </summary>
    Task<List<Expedition>> GetHistoryByFractionIdAsync(string fractionId);
}

