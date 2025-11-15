using GreatVoidBattle.Application.Dto.Ships;

namespace GreatVoidBattle.Application.Dto.Fractions;

/// <summary>
/// DTO dla widoku admina - zawiera authToken
/// </summary>
public class FractionAdminDto
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string FractionColor { get; set; } = string.Empty;
    public Guid AuthToken { get; set; }
    public bool IsDefeated { get; set; }
    public bool TurnFinished { get; set; }
    public List<ShipDto> Ships { get; set; } = [];
}
