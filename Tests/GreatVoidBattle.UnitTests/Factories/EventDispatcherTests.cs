using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Factories;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Factories;

public class EventDispatcherTests
{
    private readonly EventDispatcher _dispatcher;
    private readonly BattleState _battleState;

    public EventDispatcherTests()
    {
        _dispatcher = new EventDispatcher();
        _battleState = BattleState.CreateNew("Test Battle", 1000, 800);
    }

    [Fact]
    public async Task DispatchAsync_AddFractionEvent_ShouldAddFraction()
    {
        // Arrange
        var @event = new AddFractionEvent
        {
            BattleId = _battleState.BattleId,
            Name = "Test Faction",
            PlayerName = "Player 1",
            FractionColor = "#FF0000"
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        _battleState.Fractions.Count.ShouldBe(1);
        _battleState.Fractions.First().FractionName.ShouldBe("Test Faction");
    }

    [Fact]
    public async Task DispatchAsync_UpdateFractionEvent_ShouldUpdateFraction()
    {
        // Arrange
        var fraction = FractionState.CreateNew("Original Name", "Player 1", "#FF0000");
        _battleState.AddFraction(fraction);

        var @event = new UpdateFractionEvent
        {
            BattleId = _battleState.BattleId,
            FractionId = fraction.FractionId,
            Name = "Updated Name",
            PlayerName = "Updated Player",
            FractionColor = "#00FF00"
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        var updatedFraction = _battleState.Fractions.First();
        updatedFraction.FractionName.ShouldBe("Updated Name");
        updatedFraction.PlayerName.ShouldBe("Updated Player");
        updatedFraction.FractionColor.ShouldBe("#00FF00");
    }

    [Fact]
    public async Task DispatchAsync_AddFractionShipEvent_ShouldAddShip()
    {
        // Arrange
        var fraction = FractionState.CreateNew("Test Faction", "Player 1", "#FF0000");
        _battleState.AddFraction(fraction);

        var @event = new AddFractionShipEvent
        {
            BattleId = _battleState.BattleId,
            FractionId = fraction.FractionId,
            Name = "Test Ship",
            Type = ShipType.Corvette,
            PositionX = 100,
            PositionY = 200,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser })
            }
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        _battleState.Fractions.First().Ships.Count.ShouldBe(1);
        _battleState.Fractions.First().Ships.First().Name.ShouldBe("Test Ship");
    }

    [Fact]
    public async Task DispatchAsync_StartBattleEvent_ShouldStartBattle()
    {
        // Arrange
        var @event = new StartBattleEvent
        {
            BattleId = _battleState.BattleId
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        _battleState.BattleStatus.ShouldBe(BattleStatus.InProgress);
        _battleState.TurnNumber.ShouldBe(1);
    }

    [Fact]
    public async Task DispatchAsync_SetShipPositionEvent_ShouldUpdatePosition()
    {
        // Arrange
        var fraction = FractionState.CreateNew("Test Faction", "Player 1", "#FF0000");
        _battleState.AddFraction(fraction);

        var ship = ShipState.Create(
            fractionId: fraction.FractionId,
            name: "Test Ship",
            type: ShipType.Corvette,
            positionX: 100,
            positionY: 100,
            speed: 10,
            hitPoints: 100,
            shields: 50,
            armor: 25,
            numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: _battleState.BattleLog
        );
        _battleState.AddFractionShip(fraction.FractionId, ship);

        var @event = new SetShipPositionEvent
        {
            BattleId = _battleState.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            NewPositionX = 500,
            NewPositionY = 600
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        var updatedShip = _battleState.GetShip(ship.ShipId);
        updatedShip!.Position.X.ShouldBe(500);
        updatedShip.Position.Y.ShouldBe(600);
    }

    [Fact]
    public async Task DispatchAsync_AddShipMoveEvent_ShouldAddMovePath()
    {
        // Arrange
        var fraction = FractionState.CreateNew("Test Faction", "Player 1", "#FF0000");
        _battleState.AddFraction(fraction);

        var ship = ShipState.Create(
            fractionId: fraction.FractionId,
            name: "Test Ship",
            type: ShipType.Corvette,
            positionX: 100,
            positionY: 100,
            speed: 10,
            hitPoints: 100,
            shields: 50,
            armor: 25,
            numberOfModules: 1,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: _battleState.BattleLog
        );
        _battleState.AddFractionShip(fraction.FractionId, ship);
        _battleState.StartBattle();

        var @event = new AddShipMoveEvent
        {
            BattleId = _battleState.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            TargetPosition = new Position(500, 500)
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        _battleState.ShipMovementPaths.Count.ShouldBe(1);
        _battleState.ShipMovementPaths.First().ShipId.ShouldBe(ship.ShipId);
    }

    [Fact]
    public async Task DispatchAsync_EndOfTurnEvent_ShouldIncrementTurn()
    {
        // Arrange
        _battleState.StartBattle();
        var initialTurn = _battleState.TurnNumber;

        var @event = new EndOfTurnEvent
        {
            BattleId = _battleState.BattleId
        };

        // Act
        await _dispatcher.DispatchAsync(@event, _battleState);

        // Assert
        _battleState.TurnNumber.ShouldBe(initialTurn + 1);
    }

    [Fact]
    public async Task DispatchAsync_UnknownEventType_ShouldThrowException()
    {
        // Arrange
        var unknownEvent = new UnknownTestEvent
        {
            BattleId = _battleState.BattleId
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => _dispatcher.DispatchAsync(unknownEvent, _battleState));
        exception.Message.ShouldContain("No handler registered");
    }

    [Fact]
    public async Task DispatchAsync_MultipleEvents_ShouldApplyInOrder()
    {
        // Arrange
        var addFractionEvent = new AddFractionEvent
        {
            BattleId = _battleState.BattleId,
            Name = "Faction 1",
            PlayerName = "Player 1",
            FractionColor = "#FF0000"
        };

        var startBattleEvent = new StartBattleEvent
        {
            BattleId = _battleState.BattleId
        };

        // Act
        await _dispatcher.DispatchAsync(addFractionEvent, _battleState);
        await _dispatcher.DispatchAsync(startBattleEvent, _battleState);

        // Assert
        _battleState.Fractions.Count.ShouldBe(1);
        _battleState.BattleStatus.ShouldBe(BattleStatus.InProgress);
    }

    // Test event class that has no handler registered
    private class UnknownTestEvent : Application.Events.Base.BattleEvent
    {
    }
}

