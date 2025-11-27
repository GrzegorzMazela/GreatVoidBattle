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
    public int ResearchedTechnologiesInCurrentTier { get; set; } = 0;
    public List<FractionTechnology> Technologies { get; set; } = new();
    public List<ResearchRequest> ResearchRequests { get; set; } = new(); // Zgłoszenia graczy do zbadania
    
    // TODO: Dodać później buildings, ships, etc.
    // public List<FractionBuilding> Buildings { get; set; } = new();
    // public List<FractionShip> Ships { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Czy frakcja może awansować na wyższy tier (wymaga 15 technologii z badań na obecnym tierze)
    /// </summary>
    public bool CanAdvanceToNextTier()
    {
        return ResearchedTechnologiesInCurrentTier >= 15;
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
