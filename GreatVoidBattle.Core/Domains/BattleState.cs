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
        var missilePath = new MissileMovementPath(ship, targetShip, Const.MissileSpeed, TurnNumber);
        _missileMovementPaths.Add(missilePath);

        // Log wystrzelenia rakiety dla administratora
        BattleLog.AddTurnLog(TurnNumber, new TurnLogEntry
        {
            Type = TurnLogType.MissileFired,
            FractionId = ship.FractionId,
            FractionName = GetFraction(ship.FractionId).FractionName,
            ShipId = ship.ShipId,
            ShipName = ship.Name,
            TargetShipId = targetShip.ShipId,
            TargetShipName = targetShip.Name,
            TargetFractionId = targetShip.FractionId,
            TargetFractionName = GetFraction(targetShip.FractionId).FractionName,
            Message = $"{ship.Name} wystrzelił rakietę w {targetShip.Name}",
            AdminLog = $"[Admin] Missile {missilePath.MissileId} fired at turn {TurnNumber}, initialAccuracy={missilePath.Accuracy}, distance={missilePath.Path.Count}"
        });

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
        var currentTurn = TurnNumber;

        RunLaserShots(currentTurn);
        RunMissileShot(currentTurn);
        MoveShips(currentTurn);
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

    private void RunLaserShots(int turnNumber)
    {
        foreach (var laserShot in _laserShots)
        {
            var shootingShip = GetShip(laserShot.ShipId);
            var targetShip = GetShip(laserShot.TargetId);

            if (targetShip is null || shootingShip is null) continue;

            var (hit, rolledValue) = targetShip.TakeDamage(BattleLog, Const.LaserDamage);

            // Log dla strzelającego
            BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
            {
                Type = hit ? TurnLogType.LaserHit : TurnLogType.LaserMiss,
                FractionId = shootingShip.FractionId,
                FractionName = GetFraction(shootingShip.FractionId).FractionName,
                ShipId = shootingShip.ShipId,
                ShipName = shootingShip.Name,
                TargetShipId = targetShip.ShipId,
                TargetShipName = targetShip.Name,
                TargetFractionId = targetShip.FractionId,
                TargetFractionName = GetFraction(targetShip.FractionId).FractionName,
                Message = hit
                    ? $"{shootingShip.Name} trafił laserem w {targetShip.Name} ({Const.LaserDamage} dmg)"
                    : $"{shootingShip.Name} nie trafił laserem w {targetShip.Name}",
                AdminLog = $"[Admin] Laser shot: damage={( hit ? Const.LaserDamage : 0)}, rolledValue={rolledValue}, hit={hit}"
            });

            // Log dla trafionego (jeśli inna frakcja)
            if (hit && targetShip.FractionId != shootingShip.FractionId)
            {
                BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                {
                    Type = TurnLogType.DamageReceived,
                    FractionId = targetShip.FractionId,
                    FractionName = GetFraction(targetShip.FractionId).FractionName,
                    ShipId = targetShip.ShipId,
                    ShipName = targetShip.Name,
                    TargetShipId = shootingShip.ShipId,
                    TargetShipName = shootingShip.Name,
                    TargetFractionId = shootingShip.FractionId,
                    TargetFractionName = GetFraction(shootingShip.FractionId).FractionName,
                    Message = $"{targetShip.Name} otrzymał obrażenia od lasera {shootingShip.Name} ({Const.LaserDamage} dmg)"
                });
            }

            if (targetShip.Status == ShipStatus.Destroyed)
            {
                // Log zniszczenia dla obu stron
                BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                {
                    Type = TurnLogType.ShipDestroyed,
                    FractionId = shootingShip.FractionId,
                    FractionName = GetFraction(shootingShip.FractionId).FractionName,
                    ShipId = shootingShip.ShipId,
                    ShipName = shootingShip.Name,
                    TargetShipId = targetShip.ShipId,
                    TargetShipName = targetShip.Name,
                    TargetFractionId = targetShip.FractionId,
                    TargetFractionName = GetFraction(targetShip.FractionId).FractionName,
                    Message = $"Laser z {shootingShip.Name} zniszczył {targetShip.Name}!"
                });

                if (targetShip.FractionId != shootingShip.FractionId)
                {
                    BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                    {
                        Type = TurnLogType.ShipDestroyed,
                        FractionId = targetShip.FractionId,
                        FractionName = GetFraction(targetShip.FractionId).FractionName,
                        ShipId = targetShip.ShipId,
                        ShipName = targetShip.Name,
                        Message = $"{targetShip.Name} został zniszczony przez laser {shootingShip.Name}!"
                    });
                }

                GetFraction(targetShip.FractionId).RemoveShip(targetShip);
            }
        }
        _laserShots.Clear();
    }

    private void RunMissileShot(int turnNumber)
    {
        var completedMissiles = new List<MissileMovementPath>();
        foreach (var missileMovementPath in _missileMovementPaths)
        {
            missileMovementPath.MoveOneStep();
            if (!missileMovementPath.IsCompleted) continue;

            var shootingShip = GetShip(missileMovementPath.ShipId);
            var targetShip = GetShip(missileMovementPath.TargetId);

            if (targetShip is not null && shootingShip is not null)
            {
                var accuracy = GetAccuracy(missileMovementPath.Accuracy, targetShip);
                var (hit, rolledValue) = targetShip.TakeDamage(BattleLog, Const.MissileDamage, accuracy);

                // Log dla strzelającego z rozszerzonymi informacjami dla admina
                BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                {
                    Type = hit ? TurnLogType.MissileHit : TurnLogType.MissileMiss,
                    FractionId = shootingShip.FractionId,
                    FractionName = GetFraction(shootingShip.FractionId).FractionName,
                    ShipId = shootingShip.ShipId,
                    ShipName = shootingShip.Name,
                    TargetShipId = targetShip.ShipId,
                    TargetShipName = targetShip.Name,
                    TargetFractionId = targetShip.FractionId,
                    TargetFractionName = GetFraction(targetShip.FractionId).FractionName,
                    Message = hit
                        ? $"{shootingShip.Name} trafił rakietą w {targetShip.Name} ({Const.MissileDamage} dmg)"
                        : $"{shootingShip.Name} nie trafił rakietą w {targetShip.Name}",
                    AdminLog = $"[Admin] Missile {missileMovementPath.MissileId}: firedAt={missileMovementPath.FiredAtTurn}, hitAt={turnNumber}, travel={turnNumber - missileMovementPath.FiredAtTurn} turns, initAcc={missileMovementPath.Accuracy}, finalAcc={accuracy}, rolled={rolledValue}, hit={hit}, dmg={( hit ? Const.MissileDamage : 0)}"
                });

                // Log dla trafionego
                if (hit && targetShip.FractionId != shootingShip.FractionId)
                {
                    BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                    {
                        Type = TurnLogType.DamageReceived,
                        FractionId = targetShip.FractionId,
                        FractionName = GetFraction(targetShip.FractionId).FractionName,
                        ShipId = targetShip.ShipId,
                        ShipName = targetShip.Name,
                        TargetShipId = shootingShip.ShipId,
                        TargetShipName = shootingShip.Name,
                        TargetFractionId = shootingShip.FractionId,
                        TargetFractionName = GetFraction(shootingShip.FractionId).FractionName,
                        Message = $"{targetShip.Name} trafiony rakietą z {shootingShip.Name} ({Const.MissileDamage} dmg)"
                    });
                }

                if (targetShip.Status == ShipStatus.Destroyed)
                {
                    // Log zniszczenia
                    BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                    {
                        Type = TurnLogType.ShipDestroyed,
                        FractionId = shootingShip.FractionId,
                        FractionName = GetFraction(shootingShip.FractionId).FractionName,
                        ShipId = shootingShip.ShipId,
                        ShipName = shootingShip.Name,
                        TargetShipId = targetShip.ShipId,
                        TargetShipName = targetShip.Name,
                        TargetFractionId = targetShip.FractionId,
                        TargetFractionName = GetFraction(targetShip.FractionId).FractionName,
                        Message = $"Rakieta z {shootingShip.Name} zniszczyła {targetShip.Name}!"
                    });

                    if (targetShip.FractionId != shootingShip.FractionId)
                    {
                        BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                        {
                            Type = TurnLogType.ShipDestroyed,
                            FractionId = targetShip.FractionId,
                            FractionName = GetFraction(targetShip.FractionId).FractionName,
                            ShipId = targetShip.ShipId,
                            ShipName = targetShip.Name,
                            Message = $"{targetShip.Name} zniszczony przez rakietę z {shootingShip.Name}!"
                        });
                    }

                    GetFraction(targetShip.FractionId).RemoveShip(targetShip);
                }
            }
            completedMissiles.Add(missileMovementPath);
        }
        foreach (var completedMissile in completedMissiles)
        {
            _missileMovementPaths.Remove(completedMissile);
        }
    }

    public void MoveShips(int turnNumber)
    {
        foreach (var shipMovementPath in _shipMovementPaths)
        {
            shipMovementPath.MoveOneStep();
            var ship = GetShip(shipMovementPath.ShipId);
            if (ship is not null)
            {
                var oldPos = ship.Position;
                shipMovementPath.MoveOneStep();
                ship.UpdatePosition(shipMovementPath.StartPosition.X, shipMovementPath.StartPosition.Y);

                // Log ruchu statku
                BattleLog.AddTurnLog(turnNumber, new TurnLogEntry
                {
                    Type = TurnLogType.ShipMove,
                    FractionId = ship.FractionId,
                    FractionName = GetFraction(ship.FractionId).FractionName,
                    ShipId = ship.ShipId,
                    ShipName = ship.Name,
                    Message = $"{ship.Name} przesunął się z ({oldPos.X:F0}, {oldPos.Y:F0}) do ({ship.Position.X:F0}, {ship.Position.Y:F0})"
                });
            }

            //TODO: add logs
        }

        // Po przesunięciu statków zaktualizuj ścieżki rakiet
        UpdateMissilesPaths();
    }

    private void UpdateMissilesPaths()
    {
        var missilesToRemove = new List<MissileMovementPath>();

        foreach (var missile in _missileMovementPaths)
        {
            var targetShip = GetShip(missile.TargetId);

            // Jeśli statek docelowy nie istnieje (został zniszczony), usuń rakietę
            if (targetShip is null)
            {
                missilesToRemove.Add(missile);
                continue;
            }

            // Zaktualizuj ścieżkę rakiety do nowej pozycji statku
            try
            {
                missile.UpdateTargetPosition(targetShip.Position);
            }
            catch (InvalidOperationException)
            {
                // Jeśli cel jest teraz poza zasięgiem, usuń rakietę
                missilesToRemove.Add(missile);
            }
        }

        // Usuń rakiety, które nie mają już celu lub cel jest poza zasięgiem
        foreach (var missile in missilesToRemove)
        {
            _missileMovementPaths.Remove(missile);
        }
    }

    private int GetAccuracy(int missileAccuracy, ShipState TargetShip)
    {
        var realAccuracy = missileAccuracy - TargetShip.GetPointDefenseAccuracy();

        return realAccuracy < 20 ? 20 : realAccuracy;
    }

    #endregion InProgress
}