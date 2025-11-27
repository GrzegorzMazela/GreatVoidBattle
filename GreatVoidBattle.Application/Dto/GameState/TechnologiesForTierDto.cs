namespace GreatVoidBattle.Application.Dto.GameState;

public class TechnologiesForTierDto
{
    public int Tier { get; set; }
    public bool CanResearch { get; set; } // Czy można badać w tym tierze
    public bool CanView { get; set; } // Czy można przeglądać ten tier (preview)
    public int ResearchedCount { get; set; } // Ile zbadano z tego tieru
    public int RequiredForNextTier { get; set; } = 15; // Ile potrzeba do odblokowania następnego
    public List<TechnologyWithStatusDto> Technologies { get; set; } = new();
}

public class TechnologyWithStatusDto : TechnologyDto
{
    public bool IsOwned { get; set; }
    public bool CanResearch { get; set; } // Czy spełnione są wymagania (i tier jest dostępny)
    public bool IsPendingResearch { get; set; } // Czy już zgłoszone do badań
    public int SlotsCost { get; set; } // Ile slotów kosztuje to badanie (= tier)
    public List<string> MissingRequirements { get; set; } = new();
}
