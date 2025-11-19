using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Domains.ExtraActions;
using MongoDB.Bson.Serialization.Attributes;
using System.Net.WebSockets;

namespace GreatVoidBattle.Core.Domains;

public class BattleState
{
    [BsonId]
    public Guid BattleId { get; set; }

    public string BattleName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int TurnNumber { get; set; }
    public BattleStatus BattleStatus { get; set; }
    public BattleLog BattleLog { get; set; } = new();

    [BsonElement("Fractions")]
    private List<FractionState> _fractions { get; set; } = new();

    public IReadOnlyCollection<FractionState> Fractions => _fractions.AsReadOnly();

    [BsonElement("ShipMovementPaths")]
    private List<ShipMovementPath> _shipMovementPaths { get; set; } = new();

    public IReadOnlyCollection<ShipMovementPath> ShipMovementPaths => _shipMovementPaths.AsReadOnly();

    [BsonElement("MissileMovementPaths")]
    private List<MissileMovementPath> _missileMovementPaths { get; set; } = new();

    public IReadOnlyCollection<MissileMovementPath> MissileMovementPaths => _missileMovementPaths.AsReadOnly();

    [BsonElement("LaserShots")]
    private List<LaserShot> _laserShots { get; set; } = new();

    public IReadOnlyCollection<LaserShot> LaserShots => _laserShots.AsReadOnly();

    [BsonElement("ExtraActions")]
    private List<IExtraAction> _extraActions { get; set; } = new();

    public IReadOnlyCollection<IExtraAction> ExtraActions => _extraActions.AsReadOnly();

    public DateTime LastUpdated { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsDeleted { get; set; } = false;

    public static BattleState CreateNew(string battleName, int width, int height)
    {
        return new BattleState
        {
            BattleId = Guid.NewGuid(),
            BattleName = battleName,
            TurnNumber = 0,
            _fractions = new List<FractionState>(),
            LastUpdated = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            BattleStatus = BattleStatus.Preparation,
            Width = width,
            Height = height,
            BattleLog = new BattleLog()
        };
    }

    private FractionState GetFraction(Guid id)
    {
        var fraction = _fractions.FirstOrDefault(f => f.FractionId == id);
        if (fraction is null)
        {
            throw new InvalidOperationException($"Fraction with ID {id} not exists in the battle.");
        }
        return fraction;
    }

    public ShipState? GetShip(Guid id)
    {
        return _fractions.SelectMany(f => f.Ships)
            .FirstOrDefault(s => s.ShipId == id);
    }

    private void SetUpdated()
    {
        LastUpdated = DateTime.UtcNow;
    }

    #region Preparation

    public void AddFraction(FractionState fraction)
    {
        if (Fractions.Any(f => f.FractionId == fraction.FractionId))
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} already exists in the battle.");
        }
        _fractions.Add(fraction);
        SetUpdated();
    }

    public void AddFractionShip(Guid fractionId, ShipState shipState)
    {
        var fraction = _fractions.FirstOrDefault(f => f.FractionId == fractionId);

        if (fraction is null)
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} not exists in the battle.");
        }
        fraction.AddShip(shipState);
        SetUpdated();
    }

    public void UpdateFractionShip(Guid fractionId, ShipState updatedShipState)
    {
        var fraction = _fractions.FirstOrDefault(f => f.FractionId == fractionId);
        if (fraction is null)
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} not exists in the battle.");
        }
        fraction.UpdateShip(updatedShipState);
        SetUpdated();
    }

    public void SetNewShipPosition(Guid fractionId, Guid shipId, double newX, double newY)
    {
        var fraction = _fractions.FirstOrDefault(f => f.FractionId == fractionId);
        if (fraction is null)
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} not exists in the battle.");
        }
        var ship = fraction.Ships.FirstOrDefault(s => s.ShipId == shipId);
        if (ship is null)
        {
            throw new InvalidOperationException($"Ship with ID {ship.ShipId} not exists in the fraction.");
        }
        ship.UpdatePosition(newX, newY);
        SetUpdated();
    }

    public void StartBattle()
    {
        BattleStatus = BattleStatus.InProgress;
        SetUpdated();
        TurnNumber = 1;
    }

    #endregion Preparation

    #region InProgress

    public void AddShipMove(Guid fractionId, Guid shipId, Position targetPosition)
    {
        var ship = GetFraction(fractionId).GetShip(shipId);

        var movementPath = new ShipMovementPath(ship, targetPosition);
        movementPath.GeneratePath();
        _shipMovementPaths.RemoveAll(x => x.ShipId == shipId);
        _shipMovementPaths.Add(movementPath);
        SetUpdated();
    }

    public void AddMissileShot(Guid fractionId, Guid shipId, Guid targetFractionId, Guid targetShipId)
    {
        var ship = GetFraction(fractionId).GetShip(shipId);
        var targetShip = GetFraction(targetFractionId).GetShip(targetShipId);

        ship.FireMissile();
        var missilePath = new MissileMovementPath(ship, targetShip, Const.MissileSpeed);
        _missileMovementPaths.Add(missilePath);

        SetUpdated();
    }

    public void AddLaserShot(Guid fractionId, Guid shipId, Guid targetFractionId, Guid targetShipId)
    {
        var ship = GetFraction(fractionId).GetShip(shipId);
        var targetShip = GetFraction(targetFractionId).GetShip(targetShipId);

        ship.FireLaser();
        var laserShot = new LaserShot(ship.ShipId, targetShip.ShipId);
        _laserShots.Add(laserShot);

        SetUpdated();
    }

    public void AddExtraAction(IExtraAction extraAction)
    {
        //TODO: implement
    }

    public void EndOfTurn()
    {
        RunLaserShots();
        RunMissileShot();
        MoveShips();
        //TODO: Add extra actions processing

        foreach (var fraction in _fractions)
        {
            foreach (var ship in fraction.Ships)
            {
                ship.FinishTurn();
            }
        }
        TurnNumber++;
    }

    private void RunLaserShots()
    {
        foreach (var laserShot in _laserShots)
        {
            var targetShip = GetShip(laserShot.TargetId);
            if (targetShip is null) continue;

            targetShip.TakeDamage(BattleLog, Const.LaserDamage);
            if (targetShip.Status == ShipStatus.Destroyed)
            {
                GetFraction(targetShip.FractionId).RemoveShip(targetShip);
            }
            //TODO: add logs
        }
        _laserShots.Clear();
    }

    private void RunMissileShot()
    {
        var completedMissiles = new List<MissileMovementPath>();
        foreach (var missileMovementPath in _missileMovementPaths)
        {
            missileMovementPath.MoveOneStep();
            if (!missileMovementPath.IsCompleted) continue;

            var targetShip = GetShip(missileMovementPath.TargetId);
            if (targetShip is not null)
            {
                targetShip.TakeDamage(BattleLog, Const.MissileDamage, GetAccuracy(missileMovementPath.Accuracy, targetShip));
                if (targetShip.Status == ShipStatus.Destroyed)
                {
                    GetFraction(targetShip.FractionId).RemoveShip(targetShip);
                }
                //TODO: add logs
            }
            completedMissiles.Add(missileMovementPath);
        }
        foreach (var completedMissile in completedMissiles)
        {
            _missileMovementPaths.Remove(completedMissile);
        }
    }

    public void MoveShips()
    {
        foreach (var shipMovementPath in _shipMovementPaths)
        {
            shipMovementPath.MoveOneStep();
            var ship = GetShip(shipMovementPath.ShipId);
            if (ship is not null)
            {
                shipMovementPath.MoveOneStep();
                ship.UpdatePosition(shipMovementPath.StartPosition.X, shipMovementPath.StartPosition.Y);
            }

            //TODO: add logs
        }
    }

    private int GetAccuracy(int missileAccuracy, ShipState TargetShip)
    {
        var realAccuracy = missileAccuracy - TargetShip.GetPointDefenseAccuracy();

        return realAccuracy < 20 ? 20 : realAccuracy;
    }

    #endregion InProgress
}