namespace GreatVoidBattle.Application.Dto.Fleet;

/// <summary>
/// DTO reprezentujące pełny stan floty i układów planetarnych frakcji
/// </summary>
public class FractionFleetDto
{
    public string FractionId { get; set; } = string.Empty;
    public List<PlanetarySystemDto> PlanetarySystems { get; set; } = new();
    public List<FleetShipDto> Fleet { get; set; } = new();
    
    /// <summary>
    /// Statki nieprzypisane do żadnego układu planetarnego
    /// </summary>
    public List<FleetShipDto> UnassignedShips { get; set; } = new();
}

