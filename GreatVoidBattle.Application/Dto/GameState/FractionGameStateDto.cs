namespace GreatVoidBattle.Application.Dto.GameState;

public class FractionGameStateDto
{
    public string Id { get; set; } = string.Empty;
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public int CurrentTier { get; set; }
    public int ResearchedTechnologiesInCurrentTier { get; set; }
    public bool CanAdvanceToNextTier { get; set; }
    public List<FractionTechnologyDto> Technologies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
