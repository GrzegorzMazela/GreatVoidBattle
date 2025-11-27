namespace GreatVoidBattle.Application.Dto.Battles;

/// <summary>
/// Base class for BattleState DTOs containing common properties
/// </summary>
public abstract class BattleStateBaseDto
{
    public Guid BattleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TurnNumber { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<ShipMovementPathDto> ShipMovementPaths { get; set; } = [];
    public List<MissileMovementPathDto> MissileMovementPaths { get; set; } = [];
}

