using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Application.Mappers;
using Microsoft.AspNetCore.Mvc;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Api.Attributes;
using Microsoft.AspNetCore.SignalR;
using GreatVoidBattle.Application.Hubs;

namespace GreatVoidBattle.Application.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BattlesController : ControllerBase
{
    private readonly BattleManagerFactory _battleManagerFactory;
    private readonly IBattleStateRepository _battleStateRepository;
    private readonly IHubContext<BattleHub> _hubContext;

    public BattlesController(
        BattleManagerFactory battleManagerFactory,
        IBattleStateRepository battleStateRepository,
        IHubContext<BattleHub> hubContext)
    {
        _battleManagerFactory = battleManagerFactory;
        _battleStateRepository = battleStateRepository;
        _hubContext = hubContext;
    }

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
        var battleId = _battleManagerFactory.CreateNewBattle(createBattleEvent);
        return Ok(battleId);
    }

    [HttpGet("{battleId}")]
    [ProducesResponseType<BattleStateDto>(200)]
    public async Task<IActionResult> GetBattleState(Guid battleId)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound();
        }
        
        return Ok(BattleStateMapper.ToDto(battleState));
    }

    [HttpGet]
    public async Task<IActionResult> GetBattles(Guid battleId)
    {
        return Ok(await _battleStateRepository.GetBattles());
    }

    [HttpGet("{battleId}/admin")]
    [ProducesResponseType<BattleStateAdminDto>(200)]
    public async Task<IActionResult> GetBattleStateAdmin(Guid battleId)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound();
        }
        
        return Ok(BattleStateMapper.ToAdminDto(battleState));
    }

    [HttpPost]
    [Route("{battleId}/start")]
    public async Task<IActionResult> StartBattle(Guid battleId)
    {
        var startBattleEvent = new Application.Events.StartBattleEvent
        {
            BattleId = battleId
        };
        await _battleManagerFactory.ApplyEventAsync(startBattleEvent);
        return Ok();
    }

    [HttpPost]
    [Route("{battleId}/fractions/{fractionId}/orders")]
    [FractionAuth]
    public async Task<IActionResult> SubmitOrders(Guid battleId, Guid fractionId, [FromBody] SubmitOrdersDto submitOrdersDto)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
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
                    await _battleManagerFactory.ApplyEventAsync(moveEvent);
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
                    await _battleManagerFactory.ApplyEventAsync(laserEvent);
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
                    await _battleManagerFactory.ApplyEventAsync(missileEvent);
                    break;

                default:
                    return BadRequest($"Unknown order type: {order.Type}");
            }
        }

        // Pobierz zaktualizowany stan bitwy
        var updatedBattleState = await _battleManagerFactory.GetBattleState(battleId);
        return Ok(BattleStateMapper.ToDto(updatedBattleState));
    }

    [HttpPost]
    [Route("{battleId}/fractions/{fractionId}/end-turn")]
    [FractionAuth]
    public async Task<IActionResult> EndTurn(Guid battleId, Guid fractionId)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == fractionId);
        if (fraction == null)
        {
            return NotFound("Fraction not found");
        }

        if (fraction.TurnFinished)
        {
            return BadRequest("Turn already finished for this fraction");
        }

        // Oznacz turę frakcji jako zakończoną
        fraction.TurnFinished = true;
        await _battleStateRepository.UpdateAsync(battleState.BattleId, battleState);

        // Powiadom innych graczy że ten gracz zakończył turę
        await _hubContext.Clients.Group($"battle-{battleId}")
            .SendAsync("PlayerFinishedTurn", new
            {
                FractionId = fraction.FractionId,
                FractionName = fraction.FractionName,
                PlayerName = fraction.PlayerName,
                Timestamp = DateTime.UtcNow
            });

        // Sprawdź czy wszystkie frakcje zakończyły turę
        var allFinished = battleState.Fractions.All(f => f.IsDefeated || f.TurnFinished);

        if (allFinished)
        {
            // Wszystkie frakcje zakończyły - wykonaj turę
            var endOfTurnEvent = new Application.Events.InProgress.EndOfTurnEvent
            {
                BattleId = battleId
            };
            await _battleManagerFactory.ApplyEventAsync(endOfTurnEvent);

            // Resetuj flagi TurnFinished dla wszystkich frakcji
            var freshBattleState = await _battleManagerFactory.GetBattleState(battleId);
            foreach (var f in freshBattleState.Fractions)
            {
                f.TurnFinished = false;
            }
            await _battleStateRepository.UpdateAsync(freshBattleState.BattleId, freshBattleState);

            // Powiadom wszystkich że rozpoczyna się nowa tura
            await _hubContext.Clients.Group($"battle-{battleId}")
                .SendAsync("NewTurnStarted", new
                {
                    TurnNumber = freshBattleState.TurnNumber,
                    Timestamp = DateTime.UtcNow
                });
        }
        else
        {
            // Jeszcze nie wszyscy zakończyli - wyślij listę oczekujących
            var waitingPlayers = battleState.Fractions
                .Where(f => !f.IsDefeated && !f.TurnFinished)
                .Select(f => new
                {
                    FractionId = f.FractionId,
                    FractionName = f.FractionName,
                    PlayerName = f.PlayerName
                }).ToList();

            await _hubContext.Clients.Group($"battle-{battleId}")
                .SendAsync("WaitingPlayersUpdated", waitingPlayers);
        }

        // Zwróć listę graczy którzy jeszcze nie zakończyli
        var currentWaitingPlayers = battleState.Fractions
            .Where(f => !f.IsDefeated && !f.TurnFinished)
            .Select(f => new
            {
                FractionId = f.FractionId,
                FractionName = f.FractionName,
                PlayerName = f.PlayerName,
                TurnFinished = f.TurnFinished
            }).ToList();

        return Ok(new
        {
            TurnFinished = true,
            AllPlayersReady = allFinished,
            WaitingForPlayers = currentWaitingPlayers,
            NewTurnNumber = battleState.TurnNumber
        });
    }

    [HttpPost]
    [Route("{battleId}/execute-turn")]
    public async Task<IActionResult> ExecuteTurn(Guid battleId)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var endOfTurnEvent = new Application.Events.InProgress.EndOfTurnEvent
        {
            BattleId = battleId
        };
        await _battleManagerFactory.ApplyEventAsync(endOfTurnEvent);

        // Return updated battle state
        var updatedBattleState = await _battleManagerFactory.GetBattleState(battleId);
        return Ok(BattleStateMapper.ToDto(updatedBattleState, includeMovementPaths: false));
    }

    [HttpDelete("{battleId}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> DeleteBattle(Guid battleId)
    {
        var battleState = await _battleStateRepository.GetByIdAsync(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var deleted = await _battleStateRepository.SoftDeleteAsync(battleId);
        if (!deleted)
        {
            return NotFound("Battle could not be deleted");
        }

        return NoContent();
    }

    [HttpGet("{battleId}/fractions/{fractionId}/turn-logs/{turnNumber}")]
    [FractionAuth]
    [ProducesResponseType<TurnLogsResponseDto>(200)]
    public async Task<IActionResult> GetTurnLogs(Guid battleId, Guid fractionId, int turnNumber)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == fractionId);
        if (fraction == null)
        {
            return NotFound("Fraction not found");
        }

        var logs = battleState.BattleLog.GetTurnLogsForFraction(turnNumber, fractionId);

        var response = new TurnLogsResponseDto
        {
            TurnNumber = turnNumber,
            Logs = TurnLogMapper.ToDtoList(logs)
        };

        return Ok(response);
    }

    [HttpGet("{battleId}/admin-logs/{turnNumber}")]
    [ProducesResponseType<TurnLogsResponseDto>(200)]
    public async Task<IActionResult> GetAdminTurnLogs(Guid battleId, int turnNumber)
    {
        var battleState = await _battleManagerFactory.GetBattleState(battleId);
        if (battleState == null)
        {
            return NotFound("Battle not found");
        }

        var logs = battleState.BattleLog.GetTurnLogs(turnNumber);
        return Ok(TurnLogMapper.ToAdminDtoList(logs));
    }
}
