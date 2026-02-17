using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.GameState;

/// <summary>
/// Reprezentuje szablon statku we flocie frakcji.
/// Zawiera wszystkie parametry statku i może być użyty do tworzenia instancji statku w bitwach.
/// </summary>
public class FractionShipTemplate
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string Name { get; set; } = string.Empty;
    public ShipType Type { get; set; }
    public ShipCategory Category { get; set; } = ShipCategory.Combat;
    
    // Parametry statku
    public int Speed { get; set; }
    public int HitPoints { get; set; }
    public int Shields { get; set; }
    public int Armor { get; set; }
    public int NumberOfModules { get; set; }
    
    /// <summary>Zasięg lasera (jednostki). 0 = użyj domyślnego z Const.</summary>
    public int LaserMaxRange { get; set; }
    /// <summary>Obrażenia lasera na strzał.</summary>
    public int LaserDamage { get; set; }
    /// <summary>Maks. zasięg rakiet (Manhattan). 0 = użyj domyślnego z Const.</summary>
    public int MissileMaxRange { get; set; }
    /// <summary>Zasięg efektywny rakiet (dla celności). 0 = użyj domyślnego z Const.</summary>
    public int MissileEffectiveRange { get; set; }
    /// <summary>Obrażenia rakiety.</summary>
    public int MissileDamage { get; set; }
    /// <summary>Prędkość rakiety (komórki na turę). 0 = użyj domyślnego z Const.</summary>
    public int MissileSpeed { get; set; }
    
    /// <summary>
    /// Konfiguracja modułów - lista broni dla każdego modułu
    /// </summary>
    public List<ShipModuleTemplate> Modules { get; set; } = new();
    
    /// <summary>
    /// ID układu planetarnego, w którym stacjonuje statek (null = nie przypisany)
    /// </summary>
    public string? StationedInSystemId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Tworzy domyślny szablon statku na podstawie typu
    /// </summary>
    public static FractionShipTemplate CreateDefault(string name, ShipType type, ShipCategory category = ShipCategory.Combat)
    {
        var (speed, hitPoints, shields, armor, modules) = GetDefaultStats(type);
        
        return new FractionShipTemplate
        {
            Name = name,
            Type = type,
            Category = category,
            Speed = speed,
            HitPoints = hitPoints,
            Shields = shields,
            Armor = armor,
            NumberOfModules = modules,
            LaserMaxRange = Const.LaserMaxRange,
            LaserDamage = Const.LaserDamage,
            MissileMaxRange = Const.MissileMaxRage,
            MissileEffectiveRange = Const.MissileEffectiveRage,
            MissileDamage = Const.MissileDamage,
            MissileSpeed = Const.MissileSpeed,
            Modules = CreateDefaultModules(modules)
        };
    }
    
    private static (int speed, int hitPoints, int shields, int armor, int modules) GetDefaultStats(ShipType type)
    {
        return type switch
        {
            ShipType.Corvette => (10, 50, 25, 25, 1),
            ShipType.Destroyer => (8, 100, 50, 50, 2),
            ShipType.Cruiser => (6, 200, 100, 100, 4),
            ShipType.Battleship => (5, 400, 200, 200, 8),
            ShipType.SuperBattleship => (5, 600, 300, 300, 12),
            ShipType.OrbitalFort => (0, 100, 50, 50, 2),
            ShipType.Transport => (6, 200, 0, 100, 0),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
    
    private static List<ShipModuleTemplate> CreateDefaultModules(int count)
    {
        return Enumerable.Range(0, count)
            .Select(_ => new ShipModuleTemplate
            {
                WeaponTypes = new List<WeaponType> { WeaponType.Missile, WeaponType.Laser, WeaponType.PointDefense }
            })
            .ToList();
    }
}

/// <summary>
/// Szablon modułu statku
/// </summary>
public class ShipModuleTemplate
{
    public List<WeaponType> WeaponTypes { get; set; } = new();
}

/// <summary>
/// Kategoria statku - określa jego przeznaczenie
/// </summary>
public enum ShipCategory
{
    /// <summary>
    /// Statki bojowe - używane w symulacjach bitew
    /// </summary>
    Combat,
    
    /// <summary>
    /// Stacje orbitalne
    /// </summary>
    OrbitalStation,
    
    /// <summary>
    /// Statki badawcze
    /// </summary>
    Research
}

