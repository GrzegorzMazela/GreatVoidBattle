namespace GreatVoidBattle.Application.Dto.GameState;

public class AddTechnologyRequestDto
{
    public string FractionId { get; set; } = string.Empty;
    public string TechnologyId { get; set; } = string.Empty;
    public string Source { get; set; } = "Research"; // Research, Trade, Exchange, Other
    public string? SourceFractionId { get; set; } // Dla Trade/Exchange
    public string? SourceDescription { get; set; } // Dla Other
    public string? Comment { get; set; } // Komentarz admina
}
