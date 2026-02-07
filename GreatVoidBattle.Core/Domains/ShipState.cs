using GreatVoidBattle.Core.Domains.Enums;
using MongoDB.Bson.Serialization.Attributes;

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

    /// <summary>Zasięg lasera. 0 = użyj Const.LaserMaxRange.</summary>
    public int LaserMaxRange { get; private set; }
    /// <summary>Obrażenia lasera na strzał.</summary>
    public int LaserDamage { get; private set; }
    /// <summary>Maks. zasięg rakiet (Manhattan). 0 = użyj Const.MissileMaxRage.</summary>
    public int MissileMaxRange { get; private set; }
    /// <summary>Zasięg efektywny rakiet (celność). 0 = użyj Const.MissileEffectiveRage.</summary>
    public int MissileEffectiveRange { get; private set; }
    /// <summary>Obrażenia rakiety.</summary>
    public int MissileDamage { get; private set; }
    /// <summary>Prędkość rakiety (komórki na turę). 0 = użyj Const.MissileSpeed.</summary>
    public int MissileSpeed { get; private set; }

    [BsonElement("Modules")]
    public List<ModuleState> Modules { get; set; } = new();

    [BsonElement("Energy")]
    public EnergyDistribution Energy { get; set; } = new();

    public ShipStatus Status { get; set; } = ShipStatus.Active;

    // create statict created method
    public static ShipState Create(Guid fractionId, string name, ShipType type, double positionX, double positionY,
        int speed, int hitPoints, int shields, int armor, int numberOfModules, List<ModuleState> modules, BattleLog battleLog,
        int laserMaxRange = 0, int laserDamage = 0, int missileMaxRange = 0, int missileEffectiveRange = 0, int missileDamage = 0, int missileSpeed = 0)
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
            LaserMaxRange = laserMaxRange > 0 ? laserMaxRange : Const.LaserMaxRange,
            LaserDamage = laserDamage > 0 ? laserDamage : Const.LaserDamage,
            MissileMaxRange = missileMaxRange > 0 ? missileMaxRange : Const.MissileMaxRage,
            MissileEffectiveRange = missileEffectiveRange > 0 ? missileEffectiveRange : Const.MissileEffectiveRage,
            MissileDamage = missileDamage > 0 ? missileDamage : Const.MissileDamage,
            MissileSpeed = missileSpeed > 0 ? missileSpeed : Const.MissileSpeed
        };
    }

    public void UpdateName(string name)
    {
        Name = name;
    }

    public void UpdateType(ShipType type, List<ModuleState> modules)
    {
        Type = type;
        Modules = modules;
    }

    public void UpdateWeaponStats(int laserMaxRange, int laserDamage, int missileMaxRange, int missileEffectiveRange, int missileDamage, int missileSpeed)
    {
        LaserMaxRange = laserMaxRange > 0 ? laserMaxRange : Const.LaserMaxRange;
        LaserDamage = laserDamage > 0 ? laserDamage : Const.LaserDamage;
        MissileMaxRange = missileMaxRange > 0 ? missileMaxRange : Const.MissileMaxRage;
        MissileEffectiveRange = missileEffectiveRange > 0 ? missileEffectiveRange : Const.MissileEffectiveRage;
        MissileDamage = missileDamage > 0 ? missileDamage : Const.MissileDamage;
        MissileSpeed = missileSpeed > 0 ? missileSpeed : Const.MissileSpeed;
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

    public (bool hit, int rolledValue) TakeDamage(BattleLog battleLog, int damage, int accuracy = 100)
    {
        var rand = new Random(DateTime.UtcNow.Microsecond);
        var shotAccuracy = rand.Next(1, 101);
        if (shotAccuracy > accuracy)
        {
            battleLog.AddMissedLog(ShipId, Name, accuracy, shotAccuracy);
            return (false, shotAccuracy);
        }

        battleLog.AddTakeDamageLog(ShipId, Name, damage, accuracy, shotAccuracy);
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
            battleLog.ShipIsDestroyLog(ShipId, Name);
        }
        return (true, shotAccuracy);
    }

    public int GetPointDefenseAccuracy()
    {
        return Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.PointDefense) * 2);
    }
}