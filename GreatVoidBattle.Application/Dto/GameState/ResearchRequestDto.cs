namespace GreatVoidBattle.Application.Dto.GameState;

public class ResearchRequestDto
{
    public string TechnologyId { get; set; } = string.Empty;
    public string TechnologyName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? AdminComment { get; set; }
}

public class FractionResearchRequestsDto
{
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public List<ResearchRequestDto> PendingRequests { get; set; } = new();
}

public class TurnResolutionDto
{
    public string FractionId { get; set; } = string.Empty;
    public string TechnologyId { get; set; } = string.Empty;
    public bool Approved { get; set; }
    public string? Comment { get; set; }
}
