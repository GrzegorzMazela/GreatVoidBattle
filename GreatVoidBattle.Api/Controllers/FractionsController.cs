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
    public async Task<IActionResult> AddFraction(Guid battleId, [FromBody] CreateFractionDto createFractionDto)
    {
        var addFractionEvent = new Application.Events.AddFractionEvent
        {
            BattleId = battleId,
            Name = createFractionDto.FractionName
        };
        await battleManagerFactory.ApplyEventAsync(addFractionEvent);
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
}