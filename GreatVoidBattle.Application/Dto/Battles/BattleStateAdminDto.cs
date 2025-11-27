using GreatVoidBattle.Application.Dto.Fractions;

namespace GreatVoidBattle.Application.Dto.Battles;

/// <summary>
/// DTO dla widoku admina - zawiera authToken dla frakcji
/// </summary>
public class BattleStateAdminDto : BattleStateBaseDto
{
    public List<FractionAdminDto> Fractions { get; set; } = [];
}
