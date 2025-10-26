using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class ShipState
{
    public Guid ShipId { get; private set; }
    public Guid FractionId { get; private set; }
    public string Name { get; private set; }
    public ShipType Type { get; private set; }

    public Position Position { get; private set; } = new(0, 0);
    public int Speed { get; private set; }

    public int HitPoints { get; private set; }
    public int Shields { get; private set; }
    public int Armor { get; private set; }
    public int NumberOfModules { get; private set; }

    public int NumberOfMissiles => Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.Missile));

    public int NumberOfMissilesFiredPerTurn { get; private set; } = 0;
    public int NumberOfPointsDefense => Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.PointDefense));

    public int NumberOfLasers => Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.Laser));
    public int NumberOfLasersFiredPerTurn { get; private set; } = 0;

    public List<ModuleState> Modules { get; set; } = new();
    public EnergyDistribution Energy { get; set; } = new();

    public ShipStatus Status { get; set; } = ShipStatus.Active;
    private BattleLog _battleLog;

    // create statict created method
    public static ShipState Create(Guid fractionId, string name, ShipType type, double positionX, double positionY,
        int speed, int hitPoints, int shields, int armor, int numberOfModules, List<ModuleState> modules, BattleLog battleLog)
    {
        if (numberOfModules != modules.Count)
        {
            throw new ArgumentException("Number of modules does not match the actual modules provided.");
        }

        return new ShipState
        {
            ShipId = Guid.NewGuid(),
            FractionId = fractionId,
            Name = name,
            Type = type,
            Position = new Position(positionX, positionY),
            Speed = speed,
            HitPoints = hitPoints,
            Shields = shields,
            Armor = armor,
            Modules = modules,
            NumberOfModules = numberOfModules,
            Energy = new EnergyDistribution(),
            Status = ShipStatus.Active,
            _battleLog = battleLog
        };
    }

    public void UpdatePosition(double newX, double newY)
    {
        Position = new Position(newX, newY);
    }

    public void FireLaser()
    {
        if (NumberOfLasersFiredPerTurn >= NumberOfLasers)
        {
            throw new InvalidOperationException("No more lasers available to fire this turn.");
        }
        NumberOfLasersFiredPerTurn++;
    }

    public void FireMissile()
    {
        if (NumberOfMissilesFiredPerTurn >= NumberOfMissiles)
        {
            throw new InvalidOperationException("No more missiles available to fire this turn.");
        }
        NumberOfMissilesFiredPerTurn++;
    }

    public void FinishTurn()
    {
        NumberOfLasersFiredPerTurn = 0;
        NumberOfMissilesFiredPerTurn = 0;
    }

    public bool TakeDamage(int damage, int accuracy = 100)
    {
        var rand = new Random(DateTime.UtcNow.Microsecond);
        var shotAccuracy = rand.Next(1, 101);
        if (shotAccuracy > accuracy)
        {
            _battleLog.AddMissedLog(ShipId, Name, accuracy, shotAccuracy);
            return false;
        }

        _battleLog.AddTakeDamageLog(ShipId, Name, damage, accuracy, shotAccuracy);
        int remainingDamage = damage;
        if (Shields > 0)
        {
            if (Shields >= remainingDamage)
            {
                Shields -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= Shields;
                Shields = 0;
            }
        }
        if (remainingDamage > 0 && Armor > 0)
        {
            if (Armor >= remainingDamage)
            {
                Armor -= remainingDamage;
                remainingDamage = 0;
            }
            else
            {
                remainingDamage -= Armor;
                Armor = 0;
            }
        }
        if (remainingDamage > 0)
        {
            HitPoints -= remainingDamage;
            if (HitPoints < 0) HitPoints = 0;
        }
        if (HitPoints == 0)
        {
            Status = ShipStatus.Destroyed;
            _battleLog.ShipIsDestroyLog(ShipId, Name);
        }
        return true;
    }

    public int GetPointDefenseAccuracy()
    {
        return Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.PointDefense) * 2);
    }
}