using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Mappers;

public static class BattleStateMapper
{
    /// <summary>
    /// Maps BattleState to BattleStateDto (without sensitive data)
    /// </summary>
    public static BattleStateDto ToDto(BattleState battleState, bool includeMovementPaths = true)
    {
        var dto = new BattleStateDto
        {
            Fractions = FractionMapper.ToDtoList(battleState.Fractions)
        };

        MapBaseProperties(dto, battleState, includeMovementPaths);
        return dto;
    }

    /// <summary>
    /// Maps BattleState to BattleStateAdminDto (includes AuthTokens for fractions)
    /// </summary>
    public static BattleStateAdminDto ToAdminDto(BattleState battleState, bool includeMovementPaths = true)
    {
        var dto = new BattleStateAdminDto
        {
            Fractions = FractionMapper.ToAdminDtoList(battleState.Fractions)
        };

        MapBaseProperties(dto, battleState, includeMovementPaths);
        return dto;
    }

    /// <summary>
    /// Maps common properties to base DTO
    /// </summary>
    private static void MapBaseProperties(BattleStateBaseDto dto, BattleState battleState, bool includeMovementPaths)
    {
        dto.BattleId = battleState.BattleId;
        dto.Name = battleState.BattleName;
        dto.Status = battleState.BattleStatus.ToString();
        dto.Height = battleState.Height;
        dto.Width = battleState.Width;
        dto.TurnNumber = battleState.TurnNumber;

        if (includeMovementPaths)
        {
            dto.ShipMovementPaths = MapShipMovementPaths(battleState.ShipMovementPaths);
            dto.MissileMovementPaths = MapMissileMovementPaths(battleState.MissileMovementPaths);
        }
    }

    /// <summary>
    /// Maps ShipMovementPath collection to DTOs
    /// </summary>
    public static List<ShipMovementPathDto> MapShipMovementPaths(IEnumerable<ShipMovementPath> paths)
    {
        return paths.Select(smp => new ShipMovementPathDto
        {
            ShipId = smp.ShipId,
            Speed = smp.Speed,
            StartPosition = new PositionDto { X = smp.StartPosition.X, Y = smp.StartPosition.Y },
            TargetPosition = new PositionDto { X = smp.TargetPosition.X, Y = smp.TargetPosition.Y },
            Path = smp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
        }).ToList();
    }

    /// <summary>
    /// Maps MissileMovementPath collection to DTOs
    /// </summary>
    public static List<MissileMovementPathDto> MapMissileMovementPaths(IEnumerable<MissileMovementPath> paths)
    {
        return paths.Select(mmp => new MissileMovementPathDto
        {
            MissileId = mmp.MissileId,
            ShipId = mmp.ShipId,
            ShipName = mmp.ShipName,
            TargetId = mmp.TargetId,
            Speed = mmp.Speed,
            Accuracy = mmp.Accuracy,
            StartPosition = new PositionDto { X = mmp.StartPosition.X, Y = mmp.StartPosition.Y },
            TargetPosition = new PositionDto { X = mmp.TargetPosition.X, Y = mmp.TargetPosition.Y },
            Path = mmp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
        }).ToList();
    }
}

