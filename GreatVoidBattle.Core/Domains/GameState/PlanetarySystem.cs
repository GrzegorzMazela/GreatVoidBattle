using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.GameState;

/// <summary>
/// Reprezentuje układ planetarny należący do frakcji
/// </summary>
public class PlanetarySystem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Lista ID statków stacjonujących w tym układzie planetarnym
    /// </summary>
    public List<string> StationedShipIds { get; set; } = new();
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

