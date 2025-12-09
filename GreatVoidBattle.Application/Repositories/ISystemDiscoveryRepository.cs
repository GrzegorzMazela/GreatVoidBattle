using GreatVoidBattle.Core.Domains.Exploration;

namespace GreatVoidBattle.Application.Repositories;

public interface ISystemDiscoveryRepository
{
    Task<SystemDiscovery?> GetByIdAsync(string id);
    Task<List<SystemDiscovery>> GetAllAsync();
    Task<SystemDiscovery> CreateAsync(SystemDiscovery discovery);
    Task UpdateAsync(SystemDiscovery discovery);
    Task DeleteAsync(string id);
    
    /// <summary>
    /// Pobiera odkrycie układu dla frakcji
    /// </summary>
    Task<SystemDiscovery?> GetByFractionAndSystemAsync(string fractionId, string systemId);
    
    /// <summary>
    /// Pobiera wszystkie odkrycia frakcji
    /// </summary>
    Task<List<SystemDiscovery>> GetByFractionIdAsync(string fractionId);
    
    /// <summary>
    /// Pobiera wszystkie odkrycia danego układu (dla wszystkich frakcji)
    /// </summary>
    Task<List<SystemDiscovery>> GetBySystemIdAsync(string systemId);
    
    /// <summary>
    /// Sprawdza czy frakcja zna dany układ
    /// </summary>
    Task<bool> IsSystemKnownByFractionAsync(string fractionId, string systemId);
}

