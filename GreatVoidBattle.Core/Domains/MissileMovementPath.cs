namespace GreatVoidBattle.Core.Domains;

public class MissileMovementPath : MovementPath
{
    public Guid MissileId { get; private set; }
    public Guid ShipId { get; private set; }
    public string ShipName { get; private set; }
    public Guid TargetId { get; private set; }
    public int Accuracy { get; private set; }

    public MissileMovementPath(ShipState ship, ShipState targetShip, int speed)
        : base(speed, ship.Position, targetShip.Position)
    {
        MissileId = Guid.NewGuid();
        ShipId = ship.ShipId;
        ShipName = ship.Name;
        TargetId = targetShip.ShipId;
        GeneratePath();
        SetAccuracy();
    }

    private void SetAccuracy()
    {
        var distance = Path.Count;
        if (distance > Const.MissileMaxRage)
            throw new InvalidOperationException("Missile target is out of range.");

        if (distance > Const.MissileEffectiveRage)
        {
            Accuracy = Const.MissileAccuracy - (distance - Const.MissileEffectiveRage);
            return;
        }

        Accuracy = Const.MissileAccuracy + (Const.MissileEffectiveRage - distance);
    }
}