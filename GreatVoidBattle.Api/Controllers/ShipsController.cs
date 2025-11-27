using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Api.Attributes;
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
        var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == fractionId);
        
        if (fraction == null)
        {
            return NotFound("Fraction not found");
        }

        return Ok(ShipMapper.ToDtoList(fraction.Ships));
    }

    [HttpGet("{shipId}")]
    [ProducesResponseType<ShipDto>(200)]
    public async Task<IActionResult> GetShip(Guid battleId, Guid fractionId, Guid shipId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var shipState = battleState.GetShip(shipId);
        
        if (shipState is null)
        {
            return NotFound();
        }

        return Ok(ShipMapper.ToDto(shipState));
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
