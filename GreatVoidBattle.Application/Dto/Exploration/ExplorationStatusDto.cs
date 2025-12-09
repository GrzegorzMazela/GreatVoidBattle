namespace GreatVoidBattle.Application.Dto.Exploration;

/// <summary>
/// DTO statusu eksploracji dla frakcji
/// </summary>
public class ExplorationStatusDto
{
    public string FractionId { get; set; } = string.Empty;
    public int ResearchSlots { get; set; }
    public int UsedSlotsThisTurn { get; set; }
    public int AvailableSlots => ResearchSlots - UsedSlotsThisTurn;
    public int DiscoveredSystemsCount { get; set; }
    public int TotalExpeditions { get; set; }
    public int SuccessfulExpeditions { get; set; }
    public int PendingExpeditionsCount { get; set; }
}

/// <summary>
/// DTO historii eksploracji
/// </summary>
public class ExplorationHistoryDto
{
    public string FractionId { get; set; } = string.Empty;
    public List<ExplorationHistoryEntryDto> Entries { get; set; } = new();
}

/// <summary>
/// Wpis w historii eksploracji
/// </summary>
public class ExplorationHistoryEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // "expedition", "discovery", "info_received"
    public string SystemId { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Turn { get; set; }
    public DateTime Date { get; set; }
}

/// <summary>
/// DTO slotów badawczych dla wszystkich frakcji (admin)
/// </summary>
public class AllResearchSlotsDto
{
    public List<FractionResearchSlotsDto> Fractions { get; set; } = new();
}

/// <summary>
/// DTO slotów badawczych dla frakcji
/// </summary>
public class FractionResearchSlotsDto
{
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public int ResearchSlots { get; set; }
    public int UsedSlotsThisTurn { get; set; }
    public int PendingExpeditions { get; set; }
}

/// <summary>
/// DTO do ustawienia slotów badawczych
/// </summary>
public class SetResearchSlotsDto
{
    public int Slots { get; set; }
}

/// <summary>
/// DTO notatek admina o układzie
/// </summary>
public class SystemNotesDto
{
    public string SystemId { get; set; } = string.Empty;
    public string AdminNotes { get; set; } = string.Empty;
    public string SecretInfo { get; set; } = string.Empty;
}

