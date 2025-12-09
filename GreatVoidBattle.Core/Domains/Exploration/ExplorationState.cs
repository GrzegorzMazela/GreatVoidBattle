using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.Exploration;

/// <summary>
/// Stan eksploracji dla frakcji - ile slotów badawczych ma, statystyki itp.
/// </summary>
public class ExplorationState
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// ID frakcji
    /// </summary>
    public string FractionId { get; set; } = string.Empty;
    
    /// <summary>
    /// Ilość slotów badawczych (ile ekspedycji może wysłać na turę)
    /// </summary>
    public int ResearchSlots { get; set; } = 2;
    
    /// <summary>
    /// Ilość wykorzystanych slotów w bieżącej turze
    /// </summary>
    public int UsedSlotsThisTurn { get; set; } = 0;
    
    /// <summary>
    /// Liczba odkrytych układów
    /// </summary>
    public int DiscoveredSystemsCount { get; set; } = 0;
    
    /// <summary>
    /// Liczba wysłanych ekspedycji (wszystkich)
    /// </summary>
    public int TotalExpeditions { get; set; } = 0;
    
    /// <summary>
    /// Liczba udanych ekspedycji
    /// </summary>
    public int SuccessfulExpeditions { get; set; } = 0;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

