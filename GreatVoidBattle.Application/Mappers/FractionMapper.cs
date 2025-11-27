using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Mappers;

public static class FractionMapper
{
    /// <summary>
    /// Maps FractionState to FractionDto (without sensitive data like AuthToken)
    /// </summary>
    public static FractionDto ToDto(FractionState fraction, bool includeFullShipDetails = true)
    {
        return new FractionDto
        {
            FractionId = fraction.FractionId,
            FractionName = fraction.FractionName,
            PlayerName = fraction.PlayerName,
            FractionColor = fraction.FractionColor,
            IsDefeated = fraction.IsDefeated,
            TurnFinished = fraction.TurnFinished,
            Ships = includeFullShipDetails 
                ? ShipMapper.ToDtoList(fraction.Ships)
                : ShipMapper.ToBasicDtoList(fraction.Ships)
        };
    }

    /// <summary>
    /// Maps FractionState to FractionAdminDto (includes AuthToken)
    /// </summary>
    public static FractionAdminDto ToAdminDto(FractionState fraction)
    {
        return new FractionAdminDto
        {
            FractionId = fraction.FractionId,
            FractionName = fraction.FractionName,
            PlayerName = fraction.PlayerName,
            FractionColor = fraction.FractionColor,
            AuthToken = fraction.AuthToken,
            IsDefeated = fraction.IsDefeated,
            TurnFinished = fraction.TurnFinished,
            Ships = ShipMapper.ToDtoList(fraction.Ships)
        };
    }

    /// <summary>
    /// Maps collection of FractionState to collection of FractionDto
    /// </summary>
    public static List<FractionDto> ToDtoList(IEnumerable<FractionState> fractions, bool includeFullShipDetails = true)
    {
        return fractions.Select(f => ToDto(f, includeFullShipDetails)).ToList();
    }

    /// <summary>
    /// Maps collection of FractionState to collection of FractionAdminDto
    /// </summary>
    public static List<FractionAdminDto> ToAdminDtoList(IEnumerable<FractionState> fractions)
    {
        return fractions.Select(ToAdminDto).ToList();
    }
}

