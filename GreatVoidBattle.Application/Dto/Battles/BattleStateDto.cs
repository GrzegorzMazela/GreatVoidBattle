using GreatVoidBattle.Application.Dto.Fractions;

namespace GreatVoidBattle.Application.Dto.Battles;

/// <summary>
/// DTO for regular battle state view (without sensitive data like AuthTokens)
/// </summary>
public class BattleStateDto : BattleStateBaseDto
{
    public List<FractionDto> Fractions { get; set; } = [];
}
