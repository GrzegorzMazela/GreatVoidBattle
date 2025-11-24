using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Api.Attributes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Application.Controllers;

[Route("api/battles/{battleId}/fractions/{fractionId}/[controller]")]
[ApiController]
public class ShipsController(BattleManagerFactory battleManagerFactory) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AddShip(Guid battleId, Guid fractionId, [FromBody] CreateShipDto createShipDto)
    {
        var addShipEvent = new Application.Events.AddFractionShipEvent
        {
            BattleId = battleId,
            FractionId = fractionId,
            Name = createShipDto.Name,
            PositionX = createShipDto.PositionX,
            PositionY = createShipDto.PositionY,
            Type = Enum.Parse<ShipType>(createShipDto.Type),
            Modules = createShipDto.Modules.Select(m => new Application.Events.Module(m.WeaponTypes.Select(wt =>
                Enum.Parse<WeaponType>(wt)).ToList())
            ).ToList()
        };
        await battleManagerFactory.ApplyEventAsync(addShipEvent);
        return Ok();
    }

    [HttpPut("{shipId}")]
    public async Task<IActionResult> UpdateShip(Guid battleId, Guid fractionId, Guid shipId, [FromBody] CreateShipDto createShipDto)
    {
        var updateShipEvent = new Application.Events.UpdateFractionShipEvent
        {
            BattleId = battleId,
            FractionId = fractionId,
            ShipId = shipId,
            Name = createShipDto.Name,
            PositionX = createShipDto.PositionX,
            PositionY = createShipDto.PositionY,
            Type = Enum.Parse<ShipType>(createShipDto.Type),
            Modules = createShipDto.Modules.Select(m => new Application.Events.Module(m.WeaponTypes.Select(wt =>
                Enum.Parse<WeaponType>(wt)).ToList())
            ).ToList()
        };
        await battleManagerFactory.ApplyEventAsync(updateShipEvent);
        return Ok();
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<ShipDto>>(200)]
    public async Task<IActionResult> GetShips(Guid battleId, Guid fractionId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var ships = battleState.Fractions
            .FirstOrDefault(f => f.FractionId == fractionId)?
            .Ships.Select(s => new ShipDto
            {
                ShipId = s.ShipId,
                Name = s.Name,
                Type = s.Type.ToString(),
                X = s.Position.X,
                Y = s.Position.Y,
                Speed = s.Speed,
                Armor = s.Armor,
                Shields = s.Shields,
                HitPoints = s.HitPoints,
                Modules = s.Modules.Select(m => new ModuleDto(m.Slots.Select(slot => slot.WeaponType!.ToString()!).ToList())).ToList(),
                NumberOfMissiles = s.NumberOfMissiles,
                NumberOfLasers = s.NumberOfLasers,
                NumberOfPointsDefense = s.NumberOfPointsDefense,
                MissileMaxRange = Core.Domains.Const.MissileMaxRage,
                MissileEffectiveRange = Core.Domains.Const.MissileEffectiveRage,
                LaserMaxRange = Core.Domains.Const.LaserMaxRange
            });
        return Ok(ships);
    }

    [HttpGet("{shipId}")]
    [ProducesResponseType<IEnumerable<ShipDto>>(200)]
    public async Task<IActionResult> GetShip(Guid battleId, Guid fractionId, Guid shipId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var shipState = battleState.GetShip(shipId);
        if (shipState is null)
        {
            return NotFound();
        }

        return Ok(new ShipDto
        {
            ShipId = shipState.ShipId,
            Name = shipState.Name,
            Type = shipState.Type.ToString(),
            X = shipState.Position.X,
            Y = shipState.Position.Y,
            Speed = shipState.Speed,
            Armor = shipState.Armor,
            Shields = shipState.Shields,
            HitPoints = shipState.HitPoints,
            Modules = shipState.Modules.Select(m => new ModuleDto(m.Slots.Select(slot => slot.WeaponType!.ToString()!).ToList())).ToList(),
            NumberOfMissiles = shipState.NumberOfMissiles,
            NumberOfLasers = shipState.NumberOfLasers,
            NumberOfPointsDefense = shipState.NumberOfPointsDefense,
            MissileMaxRange = Core.Domains.Const.MissileMaxRage,
            MissileEffectiveRange = Core.Domains.Const.MissileEffectiveRage,
            LaserMaxRange = Core.Domains.Const.LaserMaxRange
        });
    }

    [HttpPatch("{shipId}/position")]
    public async Task<IActionResult> SetShipPosition(Guid battleId, Guid fractionId, Guid shipId, [FromBody] SetShipPositionDto dto)
    {
        var setPositionEvent = new Application.Events.SetShipPositionEvent
        {
            BattleId = battleId,
            FractionId = fractionId,
            ShipId = shipId,
            NewPositionX = dto.X,
            NewPositionY = dto.Y
        };
        await battleManagerFactory.ApplyEventAsync(setPositionEvent);
        return Ok();
    }
}