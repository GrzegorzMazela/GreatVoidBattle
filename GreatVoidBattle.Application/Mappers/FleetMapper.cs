using GreatVoidBattle.Application.Dto.Fleet;
using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Core.Domains.GameState;

namespace GreatVoidBattle.Application.Mappers;

public static class FleetMapper
{
    public static FleetShipDto ToDto(FractionShipTemplate ship, List<PlanetarySystem>? systems = null)
    {
        var systemName = ship.StationedInSystemId != null && systems != null
            ? systems.FirstOrDefault(s => s.Id == ship.StationedInSystemId)?.Name
            : null;
            
        return new FleetShipDto
        {
            Id = ship.Id,
            Name = ship.Name,
            Type = ship.Type.ToString(),
            Category = ship.Category.ToString(),
            Speed = ship.Speed,
            HitPoints = ship.HitPoints,
            Shields = ship.Shields,
            Armor = ship.Armor,
            NumberOfModules = ship.NumberOfModules,
            Modules = ship.Modules.Select(m => new ModuleDto(
                m.WeaponTypes.Select(wt => wt.ToString()).ToList()
            )).ToList(),
            StationedInSystemId = ship.StationedInSystemId,
            StationedInSystemName = systemName
        };
    }
    
    public static List<FleetShipDto> ToDtoList(IEnumerable<FractionShipTemplate> ships, List<PlanetarySystem>? systems = null)
    {
        return ships.Select(s => ToDto(s, systems)).ToList();
    }
    
    public static PlanetarySystemDto ToDto(PlanetarySystem system, List<FractionShipTemplate>? allShips = null)
    {
        var stationedShips = allShips?
            .Where(s => s.StationedInSystemId == system.Id)
            .Select(s => ToDto(s))
            .ToList() ?? new List<FleetShipDto>();
            
        return new PlanetarySystemDto
        {
            Id = system.Id,
            Name = system.Name,
            Description = system.Description,
            StationedShipIds = system.StationedShipIds,
            StationedShips = stationedShips
        };
    }
    
    public static List<PlanetarySystemDto> ToDtoList(IEnumerable<PlanetarySystem> systems, List<FractionShipTemplate>? allShips = null)
    {
        return systems.Select(s => ToDto(s, allShips)).ToList();
    }
    
    public static FractionFleetDto ToFractionFleetDto(FractionGameState state)
    {
        var fleet = ToDtoList(state.Fleet, state.PlanetarySystems);
        var systems = ToDtoList(state.PlanetarySystems, state.Fleet);
        var unassignedShips = fleet.Where(s => s.StationedInSystemId == null).ToList();
        
        return new FractionFleetDto
        {
            FractionId = state.FractionId,
            PlanetarySystems = systems,
            Fleet = fleet,
            UnassignedShips = unassignedShips
        };
    }
}

