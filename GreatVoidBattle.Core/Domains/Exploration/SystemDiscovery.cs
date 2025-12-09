using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.Exploration;

/// <summary>
/// Reprezentuje odkrycie układu przez frakcję - jakie informacje frakcja posiada o danym układzie
/// </summary>
public class SystemDiscovery
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// ID frakcji
    /// </summary>
    public string FractionId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID układu gwiezdnego
    /// </summary>
    public string SystemId { get; set; } = string.Empty;
    
    /// <summary>
    /// Poziom wiedzy o układzie (1-5)
    /// </summary>
    public int KnowledgeLevel { get; set; } = 1;
    
    /// <summary>
    /// Czy układ jest w pełni zbadany
    /// </summary>
    public bool FullyExplored { get; set; }
    
    /// <summary>
    /// Znana nazwa układu
    /// </summary>
    public string KnownName { get; set; } = string.Empty;
    
    /// <summary>
    /// Znany typ układu
    /// </summary>
    public string KnownType { get; set; } = string.Empty;
    
    /// <summary>
    /// Znany opis układu
    /// </summary>
    public string KnownDescription { get; set; } = string.Empty;
    
    /// <summary>
    /// Znane zasoby
    /// </summary>
    public string KnownResources { get; set; } = string.Empty;
    
    /// <summary>
    /// Znane anomalie
    /// </summary>
    public string KnownAnomalies { get; set; } = string.Empty;
    
    /// <summary>
    /// Znane połączenia do innych układów
    /// </summary>
    public List<string> KnownConnections { get; set; } = new();
    
    /// <summary>
    /// Notatki frakcji o układzie
    /// </summary>
    public string FractionNotes { get; set; } = string.Empty;
    
    /// <summary>
    /// Źródło informacji (ekspedycja, wymiana, admin)
    /// </summary>
    public DiscoverySource Source { get; set; } = DiscoverySource.Expedition;
    
    /// <summary>
    /// ID ekspedycji która odkryła układ (jeśli dotyczy)
    /// </summary>
    public string? ExpeditionId { get; set; }
    
    public DateTime DiscoveredAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum DiscoverySource
{
    Expedition,     // Odkryty przez ekspedycję
    Trade,          // Informacja wymieniona z inną frakcją
    Admin,          // Przyznany przez admina
    Initial         // Początkowy układ macierzysty
}

