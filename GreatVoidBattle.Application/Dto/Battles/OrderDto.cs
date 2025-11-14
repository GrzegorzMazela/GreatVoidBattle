namespace GreatVoidBattle.Application.Dto.Battles;

public class OrderDto
{
    public Guid ShipId { get; set; }
    public string Type { get; set; } = string.Empty; // "move", "laser", "missile"
    public double? TargetX { get; set; }
    public double? TargetY { get; set; }
    public Guid? TargetShipId { get; set; }
    public Guid? TargetFractionId { get; set; }
}
