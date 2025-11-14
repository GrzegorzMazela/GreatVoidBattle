namespace GreatVoidBattle.Application.Dto.Ships;

public class ShipDto
{
    public Guid ShipId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public int Speed { get; set; }
    public int Armor { get; set; }
    public int Shields { get; set; }
    public int HitPoints { get; set; }
    public List<ModuleDto> Modules { get; set; } = new();
}