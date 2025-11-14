namespace GreatVoidBattle.Application.Dto.Ships;

public class CreateShipDto
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public List<ModuleDto> Modules { get; set; } = new();
}