namespace GreatVoidBattle.Application.Dto.GameState;

public class GameSessionDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int CurrentTurn { get; set; }
    public List<string> FractionIds { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
