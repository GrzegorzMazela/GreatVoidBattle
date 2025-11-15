using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Application.Dto.Ships;

using GreatVoidBattle.Application.Dto.Fractions;

using GreatVoidBattle.Application.Factories;
using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Application.Controllers;

[Route("api/battles/{battleId}/[controller]")]
[ApiController]
public class FractionsController(BattleManagerFactory battleManagerFactory) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<FractionCreatedResponseDto>(200)]
    public async Task<IActionResult> AddFraction(Guid battleId, [FromBody] CreateFractionDto createFractionDto)
    {
        var addFractionEvent = new Application.Events.AddFractionEvent
        {
            BattleId = battleId,
            Name = createFractionDto.FractionName,
            PlayerName = createFractionDto.PlayerName,
            FractionColor = createFractionDto.FractionColor
        };
        await battleManagerFactory.ApplyEventAsync(addFractionEvent);
        
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var newFraction = battleState.Fractions.OrderByDescending(f => f.FractionId).FirstOrDefault();
        
        return Ok(new FractionCreatedResponseDto
        {
            FractionId = newFraction.FractionId,
            AuthToken = newFraction.AuthToken
        });
    }

    [HttpPut("{fractionId}")]
    [ProducesResponseType(200)]
    public async Task<IActionResult> UpdateFraction(Guid battleId, Guid fractionId, [FromBody] CreateFractionDto updateFractionDto)
    {
        var updateFractionEvent = new Application.Events.UpdateFractionEvent
        {
            BattleId = battleId,
            FractionId = fractionId,
            Name = updateFractionDto.FractionName,
            PlayerName = updateFractionDto.PlayerName,
            FractionColor = updateFractionDto.FractionColor
        };
        await battleManagerFactory.ApplyEventAsync(updateFractionEvent);
        
        return Ok();
    }

    [HttpGet]
    [ProducesResponseType<IEnumerable<FractionDto>>(200)]
    public async Task<IActionResult> GetFractions(Guid battleId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var fractions = battleState.Fractions.Select(f => new FractionDto
        {
            FractionId = f.FractionId,
            FractionName = f.FractionName,
            PlayerName = f.PlayerName,
            FractionColor = f.FractionColor,
            IsDefeated = f.IsDefeated,
            Ships = f.Ships.Select(s => new ShipDto
            {
                ShipId = s.ShipId,
                Name = s.Name,
                X = s.Position.X,
                Y = s.Position.Y,
                Armor = s.Armor,
                Shields = s.Shields,
                HitPoints = s.HitPoints
            }).ToList()
        });
        return Ok(fractions);
    }

    [HttpGet("admin")]
    [ProducesResponseType<IEnumerable<FractionAdminDto>>(200)]
    public async Task<IActionResult> GetFractionsAdmin(Guid battleId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var fractions = battleState.Fractions.Select(f => new FractionAdminDto
        {
            FractionId = f.FractionId,
            FractionName = f.FractionName,
            PlayerName = f.PlayerName,
            FractionColor = f.FractionColor,
            AuthToken = f.AuthToken,
            IsDefeated = f.IsDefeated,
            Ships = f.Ships.Select(s => new ShipDto
            {
                ShipId = s.ShipId,
                Name = s.Name,
                X = s.Position.X,
                Y = s.Position.Y,
                Armor = s.Armor,
                Shields = s.Shields,
                HitPoints = s.HitPoints
            }).ToList()
        });
        return Ok(fractions);
    }

    [HttpGet("{fractionId}")]
    [ProducesResponseType<FractionDto>(200)]
    public async Task<IActionResult> GetFraction(Guid battleId, Guid fractionId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == fractionId);
        
        if (fraction == null)
        {
            return NotFound();
        }

        var fractionDto = new FractionDto
        {
            FractionId = fraction.FractionId,
            FractionName = fraction.FractionName,
            PlayerName = fraction.PlayerName,
            FractionColor = fraction.FractionColor,
            IsDefeated = fraction.IsDefeated,
            Ships = fraction.Ships.Select(s => new ShipDto
            {
                ShipId = s.ShipId,
                Name = s.Name,
                X = s.Position.X,
                Y = s.Position.Y,
                Armor = s.Armor,
                Shields = s.Shields,
                HitPoints = s.HitPoints
            }).ToList()
        };
        
        return Ok(fractionDto);
    }
}