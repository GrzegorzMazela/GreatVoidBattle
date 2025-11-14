using GreatVoidBattle.Application.Dto.Ships;

namespace GreatVoidBattle.Application.Dto.Fractions;

public class FractionDto
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public bool IsDefeated { get; set; }
    public List<ShipDto> Ships { get; set; } = [];
}