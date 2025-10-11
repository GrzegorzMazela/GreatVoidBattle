namespace GreatVoidBattle.Events;

public abstract class BattleEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid BattleId { get; set; }
    public Guid FactionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}