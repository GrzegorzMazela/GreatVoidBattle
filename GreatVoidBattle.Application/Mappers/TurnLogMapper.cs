using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Mappers;

public static class TurnLogMapper
{
    /// <summary>
    /// Maps TurnLogEntry to TurnLogDto (for player view - without admin log)
    /// </summary>
    public static TurnLogDto ToDto(TurnLogEntry log)
    {
        return new TurnLogDto
        {
            Type = log.Type.ToString(),
            FractionId = log.FractionId,
            FractionName = log.FractionName,
            TargetFractionId = log.TargetFractionId,
            TargetFractionName = log.TargetFractionName,
            ShipId = log.ShipId,
            ShipName = log.ShipName,
            TargetShipId = log.TargetShipId,
            TargetShipName = log.TargetShipName,
            Message = log.Message
        };
    }

    /// <summary>
    /// Maps TurnLogEntry to TurnLogDto (for admin view - includes admin log)
    /// </summary>
    public static TurnLogDto ToAdminDto(TurnLogEntry log)
    {
        return new TurnLogDto
        {
            Type = log.Type.ToString(),
            FractionId = log.FractionId,
            FractionName = log.FractionName,
            TargetFractionId = log.TargetFractionId,
            TargetFractionName = log.TargetFractionName,
            ShipId = log.ShipId,
            ShipName = log.ShipName,
            TargetShipId = log.TargetShipId,
            TargetShipName = log.TargetShipName,
            Message = log.Message,
            AdminLog = log.AdminLog
        };
    }

    /// <summary>
    /// Maps collection of TurnLogEntry to collection of TurnLogDto
    /// </summary>
    public static List<TurnLogDto> ToDtoList(IEnumerable<TurnLogEntry> logs)
    {
        return logs.Select(ToDto).ToList();
    }

    /// <summary>
    /// Maps collection of TurnLogEntry to collection of TurnLogDto (admin view)
    /// </summary>
    public static List<TurnLogDto> ToAdminDtoList(IEnumerable<TurnLogEntry> logs)
    {
        return logs.Select(ToAdminDto).ToList();
    }
}

