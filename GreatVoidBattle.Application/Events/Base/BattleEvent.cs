namespace GreatVoidBattle.Application.Events.Base;

public abstract class BattleEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid BattleId { get; set; }
    public Guid? FractionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}