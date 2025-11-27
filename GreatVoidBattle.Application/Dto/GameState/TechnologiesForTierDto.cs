namespace GreatVoidBattle.Application.Dto.GameState;

public class TechnologiesForTierDto
{
    public int Tier { get; set; }
    public List<TechnologyWithStatusDto> Technologies { get; set; } = new();
}

public class TechnologyWithStatusDto : TechnologyDto
{
    public bool IsOwned { get; set; }
    public bool CanResearch { get; set; } // Czy spełnione są wymagania
    public List<string> MissingRequirements { get; set; } = new();
}
