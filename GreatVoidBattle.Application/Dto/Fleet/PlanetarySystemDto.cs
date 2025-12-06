namespace GreatVoidBattle.Application.Dto.Fleet;

public class PlanetarySystemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> StationedShipIds { get; set; } = new();
    public List<FleetShipDto> StationedShips { get; set; } = new();
}

public class CreatePlanetarySystemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public class UpdatePlanetarySystemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

