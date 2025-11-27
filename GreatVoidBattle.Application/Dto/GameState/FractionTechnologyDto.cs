namespace GreatVoidBattle.Application.Dto.GameState;

public class FractionTechnologyDto
{
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;
    public int Tier { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? SourceFractionId { get; set; }
    public string? SourceFractionName { get; set; }
    public string? SourceDescription { get; set; }
    public string? Comment { get; set; }
    public DateTime AcquiredDate { get; set; }
}
