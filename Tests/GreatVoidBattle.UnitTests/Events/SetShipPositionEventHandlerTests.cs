using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreatVoidBattle.UnitTests.Events;

public class SetShipPositionEventHandlerTests
{
    private readonly BattleManager _battleManager;
    private readonly Guid _fraction1Id;
    private readonly Guid _fraction2Id;

    public SetShipPositionEventHandlerTests()
    {
        var battleEvent = new CreateBattleEvent { Name = "Test Battle" };
        var battleState = BattleState.CreateNew(battleEvent.Name);
        var fraction1 = FractionState.CreateNew("Fraction 1");
        var fraction2 = FractionState.CreateNew("Fraction 2");
        battleState.AddFraction(fraction1);
        _fraction1Id = fraction1.FractionId;
        battleState.AddFraction(fraction2);
        _fraction2Id = fraction2.FractionId;
        _battleManager = new BattleManager(battleState);
    }

    private Guid CreateShip(Guid fractionId, double posX = 0, double posY = 0)
    {
        var ship = ShipState.Create(
            name: "Test Ship",
            type: ShipType.Corvette,
            positionX: posX,
            positionY: posY,
            speed: 1,
            hitPoints: 100,
            shields: 50,
            armor: 10,
            numberOfModules: 1,
            modules: [
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
                ]
        );
        _battleManager.BattleState.AddFractionShip(fractionId, ship);
        return ship.ShipId;
    }

    [Fact]
    public async Task UpdatesShipPosition_WhenBattleStatusIsPreparation()
    {
        // Arrange
        var shipId = CreateShip(_fraction1Id, 0, 0);
        double x = 42;
        double y = 99;
        var @event = new SetShipPositionEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fraction1Id,
            ShipId = shipId,
            NewPositionX = x,
            NewPositionY = y
        };

        // Act
        await _battleManager.ApplyEventAsync(@event);

        // Assert
        var ship = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fraction1Id)
            .Ships.First(s => s.ShipId == shipId);

        ship.PositionX.ShouldBe(x);
        ship.PositionY.ShouldBe(y);
    }

    [Fact]
    public async Task UpdatesShipPosition_WhenBattleStatusIsProgress_Exceptions()
    {
        // Arrange
        var shipId = CreateShip(_fraction1Id, 0, 0);
        double x = 42;
        double y = 99;
        var @startBattlerEvent = new StartBattleEvent
        {
            BattleId = _battleManager.BattleId
        };

        var @event = new SetShipPositionEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fraction1Id,
            ShipId = shipId,
            NewPositionX = x,
            NewPositionY = y
        };

        // Act
        await _battleManager.ApplyEventAsync(@startBattlerEvent);

        //zmien to tak aby sprawdzalo czy zwraca dobry blad: 
        await Should.ThrowAsync<WrongBattleStatusException>(async () => await _battleManager.ApplyEventAsync(@event));
    }

    [Fact]
    public async Task UpdatesShipPosition_WhenBattleStatusIsNotPreparation_TwoFraction()
    {
        // Arrange
        var ship1Id = CreateShip(_fraction1Id, 0, 0);
        var ship2Id = CreateShip(_fraction2Id, 10, 20);
        double x1 = 42;
        double y1 = 99;
        double x2 = 62;
        double y2 = 73;
        var @event1 = new SetShipPositionEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fraction1Id,
            ShipId = ship1Id,
            NewPositionX = x1,
            NewPositionY = y1
        };
        var @event2 = new SetShipPositionEvent
        {
            BattleId = _battleManager.BattleId,
            FractionId = _fraction2Id,
            ShipId = ship2Id,
            NewPositionX = x2,
            NewPositionY = y2
        };


        // Act
        await _battleManager.ApplyEventAsync(@event1);
        await _battleManager.ApplyEventAsync(@event2);

        // Assert
        var ship1 = _battleManager.BattleState.Fractions
            .First(f => f.FractionId == _fraction1Id)
            .Ships.First(s => s.ShipId == ship1Id);

        ship1.PositionX.ShouldBe(x1);
        ship1.PositionY.ShouldBe(y1);

        var ship2 = _battleManager.BattleState.Fractions
           .First(f => f.FractionId == _fraction2Id)
           .Ships.First(s => s.ShipId == ship2Id);

        ship2.PositionX.ShouldBe(x2);
        ship2.PositionY.ShouldBe(y2);
    }
}