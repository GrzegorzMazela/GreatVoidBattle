namespace GreatVoidBattle.Core.Domains;

public class MissileMovementPath : MovementPath
{
    public Guid MissileId { get; private set; }
    public Guid ShipId { get; private set; }
    public string ShipName { get; private set; }
    public Guid TargetId { get; private set; }
    public int Accuracy { get; private set; }
    public int FiredAtTurn { get; private set; }
    private readonly int _missileMaxRange;
    private readonly int _missileEffectiveRange;

    public MissileMovementPath(ShipState ship, ShipState targetShip, int speed, int missileMaxRange, int missileEffectiveRange, int firedAtTurn)
        : base(speed, ship.Position, targetShip.Position)
    {
        _missileMaxRange = missileMaxRange;
        _missileEffectiveRange = missileEffectiveRange;
        MissileId = Guid.NewGuid();
        ShipId = ship.ShipId;
        ShipName = ship.Name;
        TargetId = targetShip.ShipId;
        FiredAtTurn = firedAtTurn;
        GeneratePath();
        SetAccuracy();
    }

    /// <summary>
    /// Aktualizuje ścieżkę rakiety do nowej pozycji statku docelowego
    /// </summary>
    /// <param name="newTargetPosition">Nowa pozycja statku docelowego</param>
    public void UpdateTargetPosition(Position newTargetPosition)
    {
        // Zaktualizuj pozycję docelową
        NewTargetPosition(newTargetPosition);

        // Przelicz ścieżkę od aktualnej pozycji rakiety do nowej pozycji celu
        GeneratePath();
    }

    private void SetAccuracy()
    {
        var distance = Path.Count;
        if (distance > _missileMaxRange)
            throw new InvalidOperationException("Missile target is out of range.");

        if (distance > _missileEffectiveRange)
        {
            Accuracy = Const.MissileAccuracy - (distance - _missileEffectiveRange);
            return;
        }

        Accuracy = Const.MissileAccuracy + (_missileEffectiveRange - distance);
    }
}