using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.Exploration;

/// <summary>
/// Reprezentuje ekspedycję badawczą wysłaną przez frakcję
/// </summary>
public class Expedition
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// ID frakcji wysyłającej ekspedycję
    /// </summary>
    public string FractionId { get; set; } = string.Empty;
    
    /// <summary>
    /// ID docelowego układu gwiezdnego
    /// </summary>
    public string SystemId { get; set; } = string.Empty;
    
    /// <summary>
    /// Status ekspedycji
    /// </summary>
    public ExpeditionStatus Status { get; set; } = ExpeditionStatus.Pending;
    
    /// <summary>
    /// Tura w której wysłano ekspedycję
    /// </summary>
    public int SentAtTurn { get; set; }
    
    /// <summary>
    /// Komentarz frakcji do ekspedycji
    /// </summary>
    public string FractionComment { get; set; } = string.Empty;
    
    /// <summary>
    /// Komentarz admina / powód odrzucenia
    /// </summary>
    public string AdminComment { get; set; } = string.Empty;
    
    /// <summary>
    /// Wynik ekspedycji (wypełniany po rozpatrzeniu)
    /// </summary>
    public ExpeditionResult? Result { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}

public enum ExpeditionStatus
{
    Pending,    // Oczekuje na rozpatrzenie
    Approved,   // Zatwierdzona - sukces
    Rejected,   // Odrzucona
    Cancelled   // Anulowana przez frakcję
}

/// <summary>
/// Wynik ekspedycji
/// </summary>
public class ExpeditionResult
{
    /// <summary>
    /// Czy ekspedycja zakończyła się sukcesem
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Wiadomość dla frakcji opisująca wynik
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Odkryte informacje o układzie
    /// </summary>
    public DiscoveredSystemInfo? DiscoveredInfo { get; set; }
}

/// <summary>
/// Informacje o układzie odkryte przez ekspedycję
/// </summary>
public class DiscoveredSystemInfo
{
    public string SystemName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    public string Anomalies { get; set; } = string.Empty;
    public List<string> ConnectedSystems { get; set; } = new();
}

