namespace GreatVoidBattle.Core.Domains.GameState;

/// <summary>
/// Reprezentuje technologię posiadaną przez frakcję
/// </summary>
public class FractionTechnology
{
    public string TechnologyId { get; set; } = string.Empty;
    public TechnologySource Source { get; set; }
    public string? SourceFractionId { get; set; } // Dla Source = Trade/Exchange
    public string? SourceDescription { get; set; } // Dla Source = Other
    public string? Comment { get; set; } // Komentarz admina
    public DateTime AcquiredDate { get; set; }
}

public enum TechnologySource
{
    Research,      // Z badań
    Trade,         // Z handlu
    Exchange,      // Z wymiany
    Other          // Inna akcja
}
