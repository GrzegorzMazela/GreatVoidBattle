namespace GreatVoidBattle.Application.Dto.Fractions;

/// <summary>
/// DTO dla widoku admina - zawiera authToken
/// Dziedziczy po FractionDto, dodając pole AuthToken
/// </summary>
public class FractionAdminDto : FractionDto
{
    public Guid AuthToken { get; set; }
}
