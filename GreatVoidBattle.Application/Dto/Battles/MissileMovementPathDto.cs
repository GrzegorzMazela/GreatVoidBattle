namespace GreatVoidBattle.Application.Dto.Battles;

public class MissileMovementPathDto
{
    public Guid MissileId { get; set; }
    public Guid ShipId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public Guid TargetId { get; set; }
    public int Speed { get; set; }
    public int Accuracy { get; set; }
    public PositionDto StartPosition { get; set; } = new();
    public PositionDto TargetPosition { get; set; } = new();
    public List<PositionDto> Path { get; set; } = new();
}
