namespace GreatVoidBattle.Application.Dto.Exploration;

/// <summary>
/// DTO ekspedycji - widok dla frakcji
/// </summary>
public class ExpeditionDto
{
    public string Id { get; set; } = string.Empty;
    public string FractionId { get; set; } = string.Empty;
    public string SystemId { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int SentAtTurn { get; set; }
    public string FractionComment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// DTO ekspedycji - widok admina
/// </summary>
public class ExpeditionAdminDto : ExpeditionDto
{
    public string FractionName { get; set; } = string.Empty;
    public string AdminComment { get; set; } = string.Empty;
    public ExpeditionResultDto? Result { get; set; }
}

/// <summary>
/// DTO wyniku ekspedycji
/// </summary>
public class ExpeditionResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public DiscoveredSystemInfoDto? DiscoveredInfo { get; set; }
}

/// <summary>
/// DTO odkrytych informacji o układzie
/// </summary>
public class DiscoveredSystemInfoDto
{
    public string SystemName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    public string Anomalies { get; set; } = string.Empty;
    public List<string> ConnectedSystems { get; set; } = new();
}

/// <summary>
/// DTO do wysłania ekspedycji
/// </summary>
public class SendExpeditionDto
{
    public string SystemId { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
}

/// <summary>
/// DTO do rozpatrzenia ekspedycji przez admina
/// </summary>
public class ResolveExpeditionDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string AdminComment { get; set; } = string.Empty;
    
    /// <summary>
    /// Opcjonalne informacje do przekazania frakcji o układzie
    /// </summary>
    public DiscoveredSystemInfoDto? DiscoveredInfo { get; set; }
}

