namespace GreatVoidBattle.Application.Dto.Ships;

public class CreateShipDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public List<ModuleDto> Modules { get; set; } = new();
    
    // Opcjonalne parametry statku - jeśli null, używane są domyślne wartości
    public int? Speed { get; set; }
    public int? HitPoints { get; set; }
    public int? Shields { get; set; }
    public int? Armor { get; set; }
    
    // Opcjonalne parametry broni - 0 lub null = domyślne z Const
    public int? LaserMaxRange { get; set; }
    public int? LaserDamage { get; set; }
    public int? MissileMaxRange { get; set; }
    public int? MissileEffectiveRange { get; set; }
    public int? MissileDamage { get; set; }
    public int? MissileSpeed { get; set; }
    
    /// <summary>
    /// ID statku z floty frakcji - jeśli ustawione, kopiujemy parametry z szablonu floty
    /// </summary>
    public string? FleetShipTemplateId { get; set; }
}