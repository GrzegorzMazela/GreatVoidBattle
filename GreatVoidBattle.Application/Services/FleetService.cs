using GreatVoidBattle.Application.Dto.Fleet;
using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Domains.GameState;
using MongoDB.Bson;

namespace GreatVoidBattle.Application.Services;

public class FleetService(IFractionGameStateRepository repository)
{
    #region Fleet Ships
    
    public async Task<FractionFleetDto> GetFractionFleetAsync(string fractionId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        return FleetMapper.ToFractionFleetDto(state);
    }
    
    public async Task<List<FleetShipDto>> GetFleetShipsAsync(string fractionId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        return FleetMapper.ToDtoList(state.Fleet, state.PlanetarySystems);
    }
    
    public async Task<FleetShipDto?> GetFleetShipAsync(string fractionId, string shipId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var ship = state.Fleet.FirstOrDefault(s => s.Id == shipId);
        return ship == null ? null : FleetMapper.ToDto(ship, state.PlanetarySystems);
    }
    
    public async Task<FleetShipDto> CreateFleetShipAsync(string fractionId, CreateFleetShipDto dto)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        
        var shipType = Enum.Parse<ShipType>(dto.Type);
        var category = Enum.Parse<ShipCategory>(dto.Category);
        
        // Tworzymy statek z domyślnymi wartościami
        var ship = FractionShipTemplate.CreateDefault(dto.Name, shipType, category);
        
        // Nadpisujemy wartości jeśli podane
        if (dto.Speed.HasValue) ship.Speed = dto.Speed.Value;
        if (dto.HitPoints.HasValue) ship.HitPoints = dto.HitPoints.Value;
        if (dto.Shields.HasValue) ship.Shields = dto.Shields.Value;
        if (dto.Armor.HasValue) ship.Armor = dto.Armor.Value;
        
        if (dto.Modules != null && dto.Modules.Count > 0)
        {
            ship.Modules = dto.Modules.Select(m => new ShipModuleTemplate
            {
                WeaponTypes = m.WeaponTypes.Select(wt => Enum.Parse<WeaponType>(wt)).ToList()
            }).ToList();
            ship.NumberOfModules = ship.Modules.Count;
        }
        
        if (!string.IsNullOrEmpty(dto.StationedInSystemId))
        {
            ship.StationedInSystemId = dto.StationedInSystemId;
            // Dodaj też do listy w systemie
            var system = state.PlanetarySystems.FirstOrDefault(s => s.Id == dto.StationedInSystemId);
            if (system != null && !system.StationedShipIds.Contains(ship.Id))
            {
                system.StationedShipIds.Add(ship.Id);
            }
        }
        
        state.Fleet.Add(ship);
        state.UpdatedAt = DateTime.UtcNow;
        
        await repository.UpdateAsync(state);
        
        return FleetMapper.ToDto(ship, state.PlanetarySystems);
    }
    
    public async Task<FleetShipDto?> UpdateFleetShipAsync(string fractionId, string shipId, UpdateFleetShipDto dto)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var ship = state.Fleet.FirstOrDefault(s => s.Id == shipId);
        
        if (ship == null) return null;
        
        // Usuń statek ze starego systemu jeśli zmieniono
        if (ship.StationedInSystemId != null && ship.StationedInSystemId != dto.StationedInSystemId)
        {
            var oldSystem = state.PlanetarySystems.FirstOrDefault(s => s.Id == ship.StationedInSystemId);
            oldSystem?.StationedShipIds.Remove(ship.Id);
        }
        
        ship.Name = dto.Name;
        ship.Type = Enum.Parse<ShipType>(dto.Type);
        ship.Category = Enum.Parse<ShipCategory>(dto.Category);
        ship.Speed = dto.Speed;
        ship.HitPoints = dto.HitPoints;
        ship.Shields = dto.Shields;
        ship.Armor = dto.Armor;
        ship.Modules = dto.Modules.Select(m => new ShipModuleTemplate
        {
            WeaponTypes = m.WeaponTypes.Select(wt => Enum.Parse<WeaponType>(wt)).ToList()
        }).ToList();
        ship.NumberOfModules = ship.Modules.Count;
        ship.StationedInSystemId = dto.StationedInSystemId;
        ship.UpdatedAt = DateTime.UtcNow;
        
        // Dodaj do nowego systemu
        if (!string.IsNullOrEmpty(dto.StationedInSystemId))
        {
            var newSystem = state.PlanetarySystems.FirstOrDefault(s => s.Id == dto.StationedInSystemId);
            if (newSystem != null && !newSystem.StationedShipIds.Contains(ship.Id))
            {
                newSystem.StationedShipIds.Add(ship.Id);
            }
        }
        
        state.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(state);
        
        return FleetMapper.ToDto(ship, state.PlanetarySystems);
    }
    
    public async Task<bool> DeleteFleetShipAsync(string fractionId, string shipId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var ship = state.Fleet.FirstOrDefault(s => s.Id == shipId);
        
        if (ship == null) return false;
        
        // Usuń z systemu planetarnego
        if (ship.StationedInSystemId != null)
        {
            var system = state.PlanetarySystems.FirstOrDefault(s => s.Id == ship.StationedInSystemId);
            system?.StationedShipIds.Remove(ship.Id);
        }
        
        state.Fleet.Remove(ship);
        state.UpdatedAt = DateTime.UtcNow;
        
        await repository.UpdateAsync(state);
        return true;
    }
    
    public async Task<FleetShipDto?> AssignShipToSystemAsync(string fractionId, string shipId, AssignShipToSystemDto dto)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var ship = state.Fleet.FirstOrDefault(s => s.Id == shipId);
        
        if (ship == null) return null;
        
        // Usuń z obecnego systemu
        if (ship.StationedInSystemId != null)
        {
            var oldSystem = state.PlanetarySystems.FirstOrDefault(s => s.Id == ship.StationedInSystemId);
            oldSystem?.StationedShipIds.Remove(ship.Id);
        }
        
        ship.StationedInSystemId = dto.SystemId;
        ship.UpdatedAt = DateTime.UtcNow;
        
        // Dodaj do nowego systemu
        if (!string.IsNullOrEmpty(dto.SystemId))
        {
            var newSystem = state.PlanetarySystems.FirstOrDefault(s => s.Id == dto.SystemId);
            if (newSystem != null && !newSystem.StationedShipIds.Contains(ship.Id))
            {
                newSystem.StationedShipIds.Add(ship.Id);
            }
        }
        
        state.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(state);
        
        return FleetMapper.ToDto(ship, state.PlanetarySystems);
    }
    
    #endregion
    
    #region Planetary Systems
    
    public async Task<List<PlanetarySystemDto>> GetPlanetarySystemsAsync(string fractionId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        return FleetMapper.ToDtoList(state.PlanetarySystems, state.Fleet);
    }
    
    public async Task<PlanetarySystemDto?> GetPlanetarySystemAsync(string fractionId, string systemId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var system = state.PlanetarySystems.FirstOrDefault(s => s.Id == systemId);
        return system == null ? null : FleetMapper.ToDto(system, state.Fleet);
    }
    
    public async Task<PlanetarySystemDto> CreatePlanetarySystemAsync(string fractionId, CreatePlanetarySystemDto dto)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        
        var system = new PlanetarySystem
        {
            Id = ObjectId.GenerateNewId().ToString(),
            Name = dto.Name,
            Description = dto.Description
        };
        
        state.PlanetarySystems.Add(system);
        state.UpdatedAt = DateTime.UtcNow;
        
        await repository.UpdateAsync(state);
        
        return FleetMapper.ToDto(system, state.Fleet);
    }
    
    public async Task<PlanetarySystemDto?> UpdatePlanetarySystemAsync(string fractionId, string systemId, UpdatePlanetarySystemDto dto)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var system = state.PlanetarySystems.FirstOrDefault(s => s.Id == systemId);
        
        if (system == null) return null;
        
        system.Name = dto.Name;
        system.Description = dto.Description;
        system.UpdatedAt = DateTime.UtcNow;
        
        state.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(state);
        
        return FleetMapper.ToDto(system, state.Fleet);
    }
    
    public async Task<bool> DeletePlanetarySystemAsync(string fractionId, string systemId)
    {
        var state = await GetOrCreateStateAsync(fractionId);
        var system = state.PlanetarySystems.FirstOrDefault(s => s.Id == systemId);
        
        if (system == null) return false;
        
        // Usuń przypisania statków do tego systemu
        foreach (var ship in state.Fleet.Where(s => s.StationedInSystemId == systemId))
        {
            ship.StationedInSystemId = null;
        }
        
        state.PlanetarySystems.Remove(system);
        state.UpdatedAt = DateTime.UtcNow;
        
        await repository.UpdateAsync(state);
        return true;
    }
    
    #endregion
    
    private async Task<FractionGameState> GetOrCreateStateAsync(string fractionId)
    {
        var state = await repository.GetByFractionIdAsync(fractionId);
        if (state == null)
        {
            state = new FractionGameState { FractionId = fractionId };
            await repository.CreateAsync(state);
        }
        return state;
    }
}

