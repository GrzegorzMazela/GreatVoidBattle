namespace GreatVoidBattle.Application.Dto.Battles;

public class TurnLogDto
{
    public string Type { get; set; } = string.Empty;
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public Guid? TargetFractionId { get; set; }
    public string? TargetFractionName { get; set; }
    public Guid ShipId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public Guid? TargetShipId { get; set; }
    public string? TargetShipName { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Details { get; set; } = new();
}

public class TurnLogsResponseDto
{
    public int TurnNumber { get; set; }
    public List<TurnLogDto> Logs { get; set; } = new();
}
