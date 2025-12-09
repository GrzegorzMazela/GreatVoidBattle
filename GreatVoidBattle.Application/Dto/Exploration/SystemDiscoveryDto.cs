namespace GreatVoidBattle.Application.Dto.Exploration;

/// <summary>
/// DTO odkrycia układu - informacje które frakcja zna o układzie
/// </summary>
public class SystemDiscoveryDto
{
    public string Id { get; set; } = string.Empty;
    public string FractionId { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public int KnowledgeLevel { get; set; }
    public bool FullyExplored { get; set; }
    public string KnownName { get; set; } = string.Empty;
    public string KnownType { get; set; } = string.Empty;
    public string KnownDescription { get; set; } = string.Empty;
    public string KnownResources { get; set; } = string.Empty;
    public string KnownAnomalies { get; set; } = string.Empty;
    public List<string> KnownConnections { get; set; } = new();
    public string FractionNotes { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public DateTime DiscoveredAt { get; set; }
}

/// <summary>
/// DTO statusu odkrycia układu dla admina (per frakcja)
/// </summary>
public class SystemDiscoveryStatusDto
{
    public string SystemId { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public List<FractionDiscoveryStatusDto> Fractions { get; set; } = new();
}

/// <summary>
/// Status odkrycia układu dla konkretnej frakcji
/// </summary>
public class FractionDiscoveryStatusDto
{
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public bool IsDiscovered { get; set; }
    public int KnowledgeLevel { get; set; }
    public bool FullyExplored { get; set; }
    public DateTime? DiscoveredAt { get; set; }
}

/// <summary>
/// DTO do wysłania informacji o układzie do frakcji przez admina
/// </summary>
public class SendSystemInfoDto
{
    public string FractionId { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Informacje do przekazania
    /// </summary>
    public DiscoveredSystemInfoDto? Info { get; set; }
    
    /// <summary>
    /// Poziom wiedzy do ustawienia
    /// </summary>
    public int KnowledgeLevel { get; set; } = 1;
    
    /// <summary>
    /// Czy oznaczyć jako w pełni zbadany
    /// </summary>
    public bool FullyExplored { get; set; }
}

