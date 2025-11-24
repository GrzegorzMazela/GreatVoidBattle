using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events.InProgress;

public class AddShipMoveEventHandlerTests
{
    [Fact]
    public async Task AddShipMoveEventHandler_AddSingleMove_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);

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
            battleLog: battleState.BattleLog
        );
        battleState.AddFractionShip(fraction.FractionId, ship);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var moveEvent = new AddShipMoveEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            TargetPosition = new Position(200, 200)
        };

        // Act
        await battleManager.ApplyEventAsync(moveEvent);

        // Assert
        battleManager.BattleState.ShipMovementPaths.Count.ShouldBe(1);
        battleManager.BattleState.ShipMovementPaths.First().ShipId.ShouldBe(ship.ShipId);
    }

    [Fact]
    public async Task AddShipMoveEventHandler_ReplacePreviousMove_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);

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
            battleLog: battleState.BattleLog
        );
        battleState.AddFractionShip(fraction.FractionId, ship);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var moveEvent1 = new AddShipMoveEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            TargetPosition = new Position(200, 200)
        };

        var moveEvent2 = new AddShipMoveEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            TargetPosition = new Position(300, 300)
        };

        // Act
        await battleManager.ApplyEventAsync(moveEvent1);
        await battleManager.ApplyEventAsync(moveEvent2);

        // Assert
        battleManager.BattleState.ShipMovementPaths.Count.ShouldBe(1);
        var path = battleManager.BattleState.ShipMovementPaths.First();
        path.ShipId.ShouldBe(ship.ShipId);
        path.TargetPosition.X.ShouldBe(300);
        path.TargetPosition.Y.ShouldBe(300);
    }

    [Fact]
    public async Task AddShipMoveEventHandler_WhenBattleNotInProgress_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);

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
            battleLog: battleState.BattleLog
        );
        battleState.AddFractionShip(fraction.FractionId, ship);
        var battleManager = new BattleManager(battleState);

        var moveEvent = new AddShipMoveEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            TargetPosition = new Position(200, 200)
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(moveEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}
