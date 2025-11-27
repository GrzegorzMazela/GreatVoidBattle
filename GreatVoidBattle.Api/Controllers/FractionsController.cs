using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Application.Mappers;
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
        // Basic view without full ship details
        var fractions = FractionMapper.ToDtoList(battleState.Fractions, includeFullShipDetails: false);
        return Ok(fractions);
    }

    [HttpGet("admin")]
    [ProducesResponseType<IEnumerable<FractionAdminDto>>(200)]
    public async Task<IActionResult> GetFractionsAdmin(Guid battleId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        var fractions = FractionMapper.ToAdminDtoList(battleState.Fractions);
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

        // Full details for single fraction view
        return Ok(FractionMapper.ToDto(fraction, includeFullShipDetails: true));
    }
}
