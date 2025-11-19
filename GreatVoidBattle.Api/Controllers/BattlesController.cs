using GreatVoidBattle.Application.Dto.Fractions;
using GreatVoidBattle.Application.Dto.Battles;
using GreatVoidBattle.Application.Factories;
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
                PlayerName = f.PlayerName,
                FractionColor = f.FractionColor,
                //AuthToken = f.AuthToken,
                IsDefeated = f.IsDefeated,
                TurnFinished = f.TurnFinished,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
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
                    NumberOfMissiles = s.NumberOfMissiles,
                    NumberOfLasers = s.NumberOfLasers,
                    NumberOfPointsDefense = s.NumberOfPointsDefense
                }).ToList()
            }).ToList(),
            ShipMovementPaths = battleState.ShipMovementPaths.Select(smp => new ShipMovementPathDto
            {
                ShipId = smp.ShipId,
                Speed = smp.Speed,
                StartPosition = new PositionDto { X = smp.StartPosition.X, Y = smp.StartPosition.Y },
                TargetPosition = new PositionDto { X = smp.TargetPosition.X, Y = smp.TargetPosition.Y },
                Path = smp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList(),
            MissileMovementPaths = battleState.MissileMovementPaths.Select(mmp => new MissileMovementPathDto
            {
                MissileId = mmp.MissileId,
                ShipId = mmp.ShipId,
                ShipName = mmp.ShipName,
                TargetId = mmp.TargetId,
                Speed = mmp.Speed,
                Accuracy = mmp.Accuracy,
                StartPosition = new PositionDto { X = mmp.StartPosition.X, Y = mmp.StartPosition.Y },
                TargetPosition = new PositionDto { X = mmp.TargetPosition.X, Y = mmp.TargetPosition.Y },
                Path = mmp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList()
        };
        return Ok(battleStateDto);
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
        var battleStateDto = new BattleStateAdminDto
        {
            BattleId = battleState.BattleId,
            Name = battleState.BattleName,
            Status = battleState.BattleStatus.ToString(),
            Height = battleState.Height,
            Width = battleState.Width,
            TurnNumber = battleState.TurnNumber,
            Fractions = battleState.Fractions.Select(f => new FractionAdminDto
            {
                FractionId = f.FractionId,
                FractionName = f.FractionName,
                PlayerName = f.PlayerName,
                FractionColor = f.FractionColor,
                AuthToken = f.AuthToken,
                IsDefeated = f.IsDefeated,
                TurnFinished = f.TurnFinished,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
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
                    NumberOfMissiles = s.NumberOfMissiles,
                    NumberOfLasers = s.NumberOfLasers,
                    NumberOfPointsDefense = s.NumberOfPointsDefense
                }).ToList()
            }).ToList(),
            ShipMovementPaths = battleState.ShipMovementPaths.Select(smp => new ShipMovementPathDto
            {
                ShipId = smp.ShipId,
                Speed = smp.Speed,
                StartPosition = new PositionDto { X = smp.StartPosition.X, Y = smp.StartPosition.Y },
                TargetPosition = new PositionDto { X = smp.TargetPosition.X, Y = smp.TargetPosition.Y },
                Path = smp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList(),
            MissileMovementPaths = battleState.MissileMovementPaths.Select(mmp => new MissileMovementPathDto
            {
                MissileId = mmp.MissileId,
                ShipId = mmp.ShipId,
                ShipName = mmp.ShipName,
                TargetId = mmp.TargetId,
                Speed = mmp.Speed,
                Accuracy = mmp.Accuracy,
                StartPosition = new PositionDto { X = mmp.StartPosition.X, Y = mmp.StartPosition.Y },
                TargetPosition = new PositionDto { X = mmp.TargetPosition.X, Y = mmp.TargetPosition.Y },
                Path = mmp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList()
        };
        return Ok(battleStateDto);
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
                PlayerName = f.PlayerName,
                FractionColor = f.FractionColor,
                //AuthToken = f.AuthToken,
                IsDefeated = f.IsDefeated,
                TurnFinished = f.TurnFinished,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
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
                    NumberOfMissiles = s.NumberOfMissiles,
                    NumberOfLasers = s.NumberOfLasers,
                    NumberOfPointsDefense = s.NumberOfPointsDefense
                }).ToList()
            }).ToList(),
            ShipMovementPaths = updatedBattleState.ShipMovementPaths.Select(smp => new ShipMovementPathDto
            {
                ShipId = smp.ShipId,
                Speed = smp.Speed,
                StartPosition = new PositionDto { X = smp.StartPosition.X, Y = smp.StartPosition.Y },
                TargetPosition = new PositionDto { X = smp.TargetPosition.X, Y = smp.TargetPosition.Y },
                Path = smp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList(),
            MissileMovementPaths = updatedBattleState.MissileMovementPaths.Select(mmp => new MissileMovementPathDto
            {
                MissileId = mmp.MissileId,
                ShipId = mmp.ShipId,
                ShipName = mmp.ShipName,
                TargetId = mmp.TargetId,
                Speed = mmp.Speed,
                Accuracy = mmp.Accuracy,
                StartPosition = new PositionDto { X = mmp.StartPosition.X, Y = mmp.StartPosition.Y },
                TargetPosition = new PositionDto { X = mmp.TargetPosition.X, Y = mmp.TargetPosition.Y },
                Path = mmp.Path.Select(p => new PositionDto { X = p.X, Y = p.Y }).ToList()
            }).ToList()
        };

        return Ok(battleStateDto);
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
                PlayerName = f.PlayerName,
                FractionColor = f.FractionColor,
                //AuthToken = f.AuthToken,
                IsDefeated = f.IsDefeated,
                TurnFinished = f.TurnFinished,
                Ships = f.Ships.Select(s => new Application.Dto.Ships.ShipDto
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
                    NumberOfMissiles = s.NumberOfMissiles,
                    NumberOfLasers = s.NumberOfLasers,
                    NumberOfPointsDefense = s.NumberOfPointsDefense
                }).ToList()
            }).ToList()
        };

        return Ok(battleStateDto);
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
}