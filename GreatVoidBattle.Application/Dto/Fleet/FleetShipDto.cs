using GreatVoidBattle.Application.Dto.Ships;

namespace GreatVoidBattle.Application.Dto.Fleet;

/// <summary>
/// DTO reprezentujący statek we flocie frakcji
/// </summary>
public class FleetShipDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    
    // Parametry statku
    public int Speed { get; set; }
    public int HitPoints { get; set; }
    public int Shields { get; set; }
    public int Armor { get; set; }
    public int NumberOfModules { get; set; }
    
    public int LaserMaxRange { get; set; }
    public int LaserDamage { get; set; }
    public int MissileMaxRange { get; set; }
    public int MissileEffectiveRange { get; set; }
    public int MissileDamage { get; set; }
    public int MissileSpeed { get; set; }
    
    public List<ModuleDto> Modules { get; set; } = new();
    
    /// <summary>
    /// ID układu planetarnego, w którym stacjonuje statek
    /// </summary>
    public string? StationedInSystemId { get; set; }
    public string? StationedInSystemName { get; set; }
}

/// <summary>
/// DTO do tworzenia nowego statku we flocie
/// </summary>
public class CreateFleetShipDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = "Combat";
    
    // Parametry statku - opcjonalne, jeśli puste używane są domyślne
    public int? Speed { get; set; }
    public int? HitPoints { get; set; }
    public int? Shields { get; set; }
    public int? Armor { get; set; }
    public int? LaserMaxRange { get; set; }
    public int? LaserDamage { get; set; }
    public int? MissileMaxRange { get; set; }
    public int? MissileEffectiveRange { get; set; }
    public int? MissileDamage { get; set; }
    public int? MissileSpeed { get; set; }
    
    public List<ModuleDto>? Modules { get; set; }
    
    /// <summary>
    /// ID układu planetarnego, w którym ma stacjonować statek
    /// </summary>
    public string? StationedInSystemId { get; set; }
}

/// <summary>
/// DTO do aktualizacji statku we flocie
/// </summary>
public class UpdateFleetShipDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Category { get; set; } = "Combat";
    
    public int Speed { get; set; }
    public int HitPoints { get; set; }
    public int Shields { get; set; }
    public int Armor { get; set; }
    public int LaserMaxRange { get; set; }
    public int LaserDamage { get; set; }
    public int MissileMaxRange { get; set; }
    public int MissileEffectiveRange { get; set; }
    public int MissileDamage { get; set; }
    public int MissileSpeed { get; set; }
    
    public List<ModuleDto> Modules { get; set; } = new();
    
    public string? StationedInSystemId { get; set; }
}

/// <summary>
/// DTO do przypisania statku do układu planetarnego
/// </summary>
public class AssignShipToSystemDto
{
    public string? SystemId { get; set; } // null = usuwa przypisanie
}

