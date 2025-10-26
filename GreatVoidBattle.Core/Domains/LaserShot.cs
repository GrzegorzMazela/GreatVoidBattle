namespace GreatVoidBattle.Core.Domains;

public record LaserShot(Guid ShipId, Guid TargetId)
{
    public Guid LaserId { get; private set; } = Guid.NewGuid();
}