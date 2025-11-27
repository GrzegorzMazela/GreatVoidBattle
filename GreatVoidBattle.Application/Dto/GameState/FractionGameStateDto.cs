namespace GreatVoidBattle.Application.Dto.GameState;

public class FractionGameStateDto
{
    public string Id { get; set; } = string.Empty;
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public int CurrentTier { get; set; }
    public int ResearchSlots { get; set; }
    public int UsedResearchSlots { get; set; }
    public int AvailableResearchSlots => ResearchSlots - UsedResearchSlots;
    public int ResearchedTechnologiesInCurrentTier { get; set; }
    public bool CanAdvanceToNextTier { get; set; }
    public List<FractionTechnologyDto> Technologies { get; set; } = new();
    public List<TierProgressDto> TierProgress { get; set; } = new(); // Postęp badań w każdym tierze
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class TierProgressDto
{
    public int Tier { get; set; }
    public int ResearchedCount { get; set; }
    public int RequiredForNextTier { get; set; } = 15;
    public bool CanResearch { get; set; }
    public bool CanView { get; set; }
}
