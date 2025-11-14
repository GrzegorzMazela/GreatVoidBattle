using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Application.Factories;
using Microsoft.AspNetCore.Mvc;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Events.InProgress;

namespace GreatVoidBattle.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BattlesController(BattleManagerFactory battleManagerFactory, IBattleStateRepository _battleStateRepository) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<Guid>(201)]
    public IActionResult CreateBattle([FromBody] CreateBattleDto createBattleDto)
    {
        var createBattleEvent = new Application.Events.CreateBattleEvent
        {
            Name = createBattleDto.Name,
            Width = createBattleDto.Width,
            Height = createBattleDto.Height
        };
        var battleId = battleManagerFactory.CreateNewBattle(createBattleEvent);
        return Ok(battleId);
    }

    [HttpGet("{battleId}")]
    [ProducesResponseType<BattleStateDto>(200)]
    public async Task<IActionResult> GetBattleState(Guid battleId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound();
        }
        var battleStateDto = new BattleStateDto
        {
            BattleId = battleState.BattleId,
            Name = battleState.BattleName,
            Status = battleState.BattleStatus.ToString(),
            Height = battleState.Height,
            Width = battleState.Width,
            TurnNumber = battleState.TurnNumber,
            Fractions = battleState.Fractions.Select(f => new FractionDto
            {
                FractionId = f.FractionId,
                FractionName = f.FractionName,
                IsDefeated = f.IsDefeated,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
                {
                    ShipId = s.ShipId,
                    Name = s.Name,
                    X = s.Position.X,
                    Y = s.Position.Y,
                    Speed = s.Speed,
                    Armor = s.Armor,
                    Shields = s.Shields,
                    HitPoints = s.HitPoints
                }).ToList()
            }).ToList()
        };
        return Ok(battleStateDto);
    }

    [HttpGet]
    public async Task<IActionResult> GetBattles(Guid battleId)
    {
        return Ok(await _battleStateRepository.GetBattles());
    }

    [HttpPost]
    [Route("{battleId}/start")]
    public async Task<IActionResult> StartBattle(Guid battleId)
    {
        var startBattleEvent = new Application.Events.StartBattleEvent
        {
            BattleId = battleId
        };
        await battleManagerFactory.ApplyEventAsync(startBattleEvent);
        return Ok();
    }

    [HttpPost]
    [Route("{battleId}/fractions/{fractionId}/orders")]
    public async Task<IActionResult> SubmitOrders(Guid battleId, Guid fractionId, [FromBody] SubmitOrdersDto submitOrdersDto)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        if (battleState.TurnNumber != submitOrdersDto.TurnNumber)
        {
            return BadRequest($"Invalid turn number. Current turn: {battleState.TurnNumber}");
        }

        foreach (var order in submitOrdersDto.Orders)
        {
            switch (order.Type.ToLower())
            {
                case "move":
                    if (!order.TargetX.HasValue || !order.TargetY.HasValue)
                    {
                        return BadRequest("Move order requires TargetX and TargetY");
                    }
                    var moveEvent = new Application.Events.InProgress.AddShipMoveEvent
                    {
                        BattleId = battleId,
                        FractionId = fractionId,
                        ShipId = order.ShipId,
                        TargetPosition = new Core.Domains.Position(order.TargetX.Value, order.TargetY.Value)
                    };
                    await battleManagerFactory.ApplyEventAsync(moveEvent);
                    break;

                case "laser":
                    if (!order.TargetShipId.HasValue || !order.TargetFractionId.HasValue)
                    {
                        return BadRequest("Laser order requires TargetShipId and TargetFractionId");
                    }
                    var laserEvent = new Application.Events.InProgress.AddLaserShotEvent
                    {
                        BattleId = battleId,
                        FractionId = fractionId,
                        ShipId = order.ShipId,
                        TargetShipId = order.TargetShipId.Value,
                        TargetFractionId = order.TargetFractionId.Value
                    };
                    await battleManagerFactory.ApplyEventAsync(laserEvent);
                    break;

                case "missile":
                    if (!order.TargetShipId.HasValue || !order.TargetFractionId.HasValue)
                    {
                        return BadRequest("Missile order requires TargetShipId and TargetFractionId");
                    }
                    var missileEvent = new Application.Events.InProgress.AddMissileShotEvent
                    {
                        BattleId = battleId,
                        FractionId = fractionId,
                        ShipId = order.ShipId,
                        TargetShipId = order.TargetShipId.Value,
                        TargetFractionId = order.TargetFractionId.Value
                    };
                    await battleManagerFactory.ApplyEventAsync(missileEvent);
                    break;

                default:
                    return BadRequest($"Unknown order type: {order.Type}");
            }
        }

        return Ok(new { message = "Orders submitted successfully" });
    }

    [HttpPost]
    [Route("{battleId}/execute-turn")]
    public async Task<IActionResult> ExecuteTurn(Guid battleId)
    {
        var battleState = await battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var endOfTurnEvent = new Application.Events.InProgress.EndOfTurnEvent
        {
            BattleId = battleId
        };
        await battleManagerFactory.ApplyEventAsync(endOfTurnEvent);

        // Return updated battle state
        var updatedBattleState = await battleManagerFactory.GetBattleState(battleId);
        var battleStateDto = new BattleStateDto
        {
            BattleId = updatedBattleState.BattleId,
            Name = updatedBattleState.BattleName,
            Status = updatedBattleState.BattleStatus.ToString(),
            Height = updatedBattleState.Height,
            Width = updatedBattleState.Width,
            TurnNumber = updatedBattleState.TurnNumber,
            Fractions = updatedBattleState.Fractions.Select(f => new FractionDto
            {
                FractionId = f.FractionId,
                FractionName = f.FractionName,
                IsDefeated = f.IsDefeated,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
                {
                    ShipId = s.ShipId,
                    Name = s.Name,
                    X = s.Position.X,
                    Y = s.Position.Y,
                    Speed = s.Speed,
                    Armor = s.Armor,
                    Shields = s.Shields,
                    HitPoints = s.HitPoints
                }).ToList()
            }).ToList()
        };
        
        return Ok(battleStateDto);
    }
}