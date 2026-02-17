using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Factories;

public static class ShipFactory
{
    /// <summary>
    /// Pobiera domyślne statystyki dla danego typu statku
    /// </summary>
    public static (int speed, int hitPoints, int shields, int armor, int modules) GetDefaultStats(ShipType type)
    {
        return type switch
        {
            ShipType.Corvette => (10, 50, 25, 25, 1),
            ShipType.Destroyer => (8, 100, 50, 50, 2),
            ShipType.Cruiser => (6, 200, 100, 100, 4),
            ShipType.Battleship => (5, 400, 200, 200, 8),
            ShipType.SuperBattleship => (5, 600, 300, 300, 12),
            ShipType.OrbitalFort => (0, 100, 50, 50, 2),
            ShipType.Transport => (6, 200, 0, 100, 0), // Jak krążownik, ale 0 modułów i 0 tarcz
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    public static ShipState CreateShip(AddFractionShipEvent addShipEvent, BattleLog battleLog)
    {
        var (defaultSpeed, defaultHitPoints, defaultShields, defaultArmor, defaultModules) = GetDefaultStats(addShipEvent.Type);
        
        // Użyj wartości z eventu jeśli podane, w przeciwnym razie domyślne
        var speed = addShipEvent.Speed ?? defaultSpeed;
        var hitPoints = addShipEvent.HitPoints ?? defaultHitPoints;
        var shields = addShipEvent.Shields ?? defaultShields;
        var armor = addShipEvent.Armor ?? defaultArmor;
        var numberOfModules = addShipEvent.Modules.Count > 0 ? addShipEvent.Modules.Count : defaultModules;
        
        return CreateShipState(addShipEvent, speed, hitPoints, shields, armor, numberOfModules, battleLog);
    }

    public static ShipState CreateShipState(AddFractionShipEvent addShipEvent, int speed, int hitPoints,
        int shields, int armor, int numberOfModules, BattleLog battleLog)
    {
        var laserMaxRange = addShipEvent.LaserMaxRange ?? 0;
        var laserDamage = addShipEvent.LaserDamage ?? 0;
        var missileMaxRange = addShipEvent.MissileMaxRange ?? 0;
        var missileEffectiveRange = addShipEvent.MissileEffectiveRange ?? 0;
        var missileDamage = addShipEvent.MissileDamage ?? 0;
        var missileSpeed = addShipEvent.MissileSpeed ?? 0;
        return ShipState.Create(addShipEvent.FractionId!.Value, addShipEvent.Name, addShipEvent.Type, addShipEvent.PositionX, addShipEvent.PositionY,
            speed: speed, hitPoints: hitPoints, shields: shields, armor: armor, numberOfModules: numberOfModules,
            addShipEvent.Modules.Select(m => ModuleState.Create(m.WeaponTypes.Select(wt => SystemSlot.Create(wt)).ToList())).ToList(), battleLog,
            laserMaxRange, laserDamage, missileMaxRange, missileEffectiveRange, missileDamage, missileSpeed);
    }
}