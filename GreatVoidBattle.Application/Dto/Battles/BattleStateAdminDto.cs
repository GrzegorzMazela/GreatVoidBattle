using GreatVoidBattle.Application.Dto.Fractions;

namespace GreatVoidBattle.Application.Dto.Battles;

/// <summary>
/// DTO dla widoku admina - zawiera authToken dla frakcji
/// </summary>
public class BattleStateAdminDto
{
    public Guid BattleId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public int TurnNumber { get; set; }
    public List<FractionAdminDto> Fractions { get; set; } = [];
    public List<ShipMovementPathDto> ShipMovementPaths { get; set; } = [];
    public List<MissileMovementPathDto> MissileMovementPaths { get; set; } = [];
}
