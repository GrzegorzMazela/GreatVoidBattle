using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.GameState;

/// <summary>
/// Reprezentuje sesję gry - główny obiekt stanu gry
/// </summary>
public class GameSession
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    public int CurrentTurn { get; set; } = 1;
    public List<string> FractionIds { get; set; } = new(); // Lista ID frakcji uczestniczących w grze
    public GameSessionStatus Status { get; set; } = GameSessionStatus.Active;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public enum GameSessionStatus
{
    Active,
    Paused,
    Finished
}
