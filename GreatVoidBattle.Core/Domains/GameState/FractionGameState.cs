using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.GameState;

/// <summary>
/// Reprezentuje stan gry dla danej frakcji
/// </summary>
public class FractionGameState
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string FractionId { get; set; } = string.Empty;
    public int CurrentTier { get; set; } = 1;
    public int ResearchSlots { get; set; } = 3; // Ilość slotów badawczych
    public int ResearchedTechnologiesInCurrentTier { get; set; } = 0;
    public List<FractionTechnology> Technologies { get; set; } = new();
    public List<ResearchRequest> ResearchRequests { get; set; } = new(); // Zgłoszenia graczy do zbadania
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Czy frakcja może awansować na wyższy tier (wymaga 15 technologii z badań na obecnym tierze)
    /// </summary>
    public bool CanAdvanceToNextTier()
    {
        return ResearchedTechnologiesInCurrentTier >= 15;
    }
    
    /// <summary>
    /// Oblicza ile slotów jest obecnie używanych przez oczekujące badania
    /// Tier 1 = 1 slot, Tier 2 = 2 sloty, itd.
    /// </summary>
    public int GetUsedResearchSlots(Func<string, int> getTechnologyTier)
    {
        return ResearchRequests
            .Where(r => r.Status == ResearchRequestStatus.Pending)
            .Sum(r => getTechnologyTier(r.TechnologyId));
    }
    
    /// <summary>
    /// Sprawdza ile technologii frakcja ma zbadanych (źródło: Research) dla danego tieru
    /// </summary>
    public int GetResearchedCountForTier(Func<string, int> getTechnologyTier, int tier)
    {
        return Technologies
            .Where(t => t.Source == TechnologySource.Research && getTechnologyTier(t.TechnologyId) == tier)
            .Count();
    }
    
    /// <summary>
    /// Sprawdza czy frakcja może badać dany tier (musi mieć 15 zbadanych technologii z poprzedniego tieru)
    /// Tier 1 jest zawsze dostępny
    /// </summary>
    public bool CanResearchTier(Func<string, int> getTechnologyTier, int tier)
    {
        if (tier == 1) return true;
        return GetResearchedCountForTier(getTechnologyTier, tier - 1) >= 15;
    }
    
    /// <summary>
    /// Sprawdza czy frakcja może widzieć dany tier (dostępny tier + 1)
    /// </summary>
    public bool CanViewTier(Func<string, int> getTechnologyTier, int tier)
    {
        // Może widzieć tier jeśli może go badać LUB jeśli jest o jeden wyższy od dostępnego
        if (CanResearchTier(getTechnologyTier, tier)) return true;
        
        // Może podglądać tier + 1
        return CanResearchTier(getTechnologyTier, tier - 1);
    }
}

/// <summary>
/// Reprezentuje zgłoszenie gracza do zbadania technologii w tej turze
/// </summary>
public class ResearchRequest
{
    public string TechnologyId { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public ResearchRequestStatus Status { get; set; } = ResearchRequestStatus.Pending;
    public string? AdminComment { get; set; } // Komentarz admina przy zatwierdzaniu
}

public enum ResearchRequestStatus
{
    Pending,   // Oczekuje na zatwierdzenie
    Approved,  // Zatwierdzona przez admina
    Rejected   // Odrzucona przez admina
}
