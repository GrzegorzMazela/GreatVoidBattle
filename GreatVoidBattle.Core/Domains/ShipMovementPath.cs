namespace GreatVoidBattle.Core.Domains;

public class ShipMovementPath : MovementPath
{
    public Guid ShipId { get; private set; }

    public ShipMovementPath(ShipState ship, Position targetPosition) : base(ship.Speed, ship.Position, targetPosition)
    {
        ShipId = ship.ShipId;
    }
}