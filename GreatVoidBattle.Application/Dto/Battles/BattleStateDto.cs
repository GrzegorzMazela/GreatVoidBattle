using GreatVoidBattle.Application.Dto.Fractions;

namespace GreatVoidBattle.Application.Dto.Battles;

public class BattleStateDto
{
    public Guid BattleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TurnNumber { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public List<FractionDto> Fractions { get; set; } = new();
    public List<ShipMovementPathDto> ShipMovementPaths { get; set; } = new();
    public List<MissileMovementPathDto> MissileMovementPaths { get; set; } = new();
}