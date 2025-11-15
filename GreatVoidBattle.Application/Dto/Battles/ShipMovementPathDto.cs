namespace GreatVoidBattle.Application.Dto.Battles;

public class ShipMovementPathDto
{
    public Guid ShipId { get; set; }
    public int Speed { get; set; }
    public PositionDto StartPosition { get; set; } = new();
    public PositionDto TargetPosition { get; set; } = new();
    public List<PositionDto> Path { get; set; } = new();
}
