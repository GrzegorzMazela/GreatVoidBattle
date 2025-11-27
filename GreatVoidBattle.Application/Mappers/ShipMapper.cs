using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Mappers;

public static class ShipMapper
{
    /// <summary>
    /// Maps ShipState to ShipDto with full details (including modules and weapon info)
    /// </summary>
    public static ShipDto ToDto(ShipState ship)
    {
        return new ShipDto
        {
            ShipId = ship.ShipId,
            Name = ship.Name,
            Type = ship.Type.ToString(),
            X = ship.Position.X,
            Y = ship.Position.Y,
            Speed = ship.Speed,
            Armor = ship.Armor,
            Shields = ship.Shields,
            HitPoints = ship.HitPoints,
            Modules = ship.Modules.Select(m => new ModuleDto(
                m.Slots.Select(slot => slot.WeaponType?.ToString() ?? string.Empty).ToList()
            )).ToList(),
            NumberOfMissiles = ship.NumberOfMissiles,
            NumberOfLasers = ship.NumberOfLasers,
            NumberOfPointsDefense = ship.NumberOfPointsDefense,
            MissileMaxRange = Const.MissileMaxRage,
            MissileEffectiveRange = Const.MissileEffectiveRage,
            LaserMaxRange = Const.LaserMaxRange
        };
    }

    /// <summary>
    /// Maps ShipState to ShipDto with basic info only (for list views)
    /// </summary>
    public static ShipDto ToBasicDto(ShipState ship)
    {
        return new ShipDto
        {
            ShipId = ship.ShipId,
            Name = ship.Name,
            X = ship.Position.X,
            Y = ship.Position.Y,
            Armor = ship.Armor,
            Shields = ship.Shields,
            HitPoints = ship.HitPoints
        };
    }

    /// <summary>
    /// Maps collection of ShipState to collection of ShipDto
    /// </summary>
    public static List<ShipDto> ToDtoList(IEnumerable<ShipState> ships)
    {
        return ships.Select(ToDto).ToList();
    }

    /// <summary>
    /// Maps collection of ShipState to collection of basic ShipDto
    /// </summary>
    public static List<ShipDto> ToBasicDtoList(IEnumerable<ShipState> ships)
    {
        return ships.Select(ToBasicDto).ToList();
    }
}

