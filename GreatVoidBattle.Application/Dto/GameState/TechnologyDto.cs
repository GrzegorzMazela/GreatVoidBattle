namespace GreatVoidBattle.Application.Dto.GameState;

public class TechnologyDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Tier { get; set; }
    public List<string> RequiredTechnologies { get; set; } = new();
}
