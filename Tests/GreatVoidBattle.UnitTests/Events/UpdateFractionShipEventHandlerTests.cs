using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class UpdateFractionShipEventHandlerTests
{
    [Fact]
    public async Task UpdateFractionShipEventHandler_UpdateShipName_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);

        var ship = ShipState.Create(
            fractionId: fraction.FractionId,
            name: "Original Ship",
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

        var updateEvent = new UpdateFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            Name = "Updated Ship",
            Type = ShipType.Corvette,
            PositionX = 100,
            PositionY = 100,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser })
            }
        };

        // Act
        await battleManager.ApplyEventAsync(updateEvent);

        // Assert
        var updatedShip = battleManager.BattleState.Fractions.First().Ships.First();
        updatedShip.Name.ShouldBe("Updated Ship");
    }

    [Fact]
    public async Task UpdateFractionShipEventHandler_UpdateShipType_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
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

        var updateEvent = new UpdateFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            Name = "Test Ship",
            Type = ShipType.Destroyer,
            PositionX = 100,
            PositionY = 100,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser }),
                new Module(new List<WeaponType> { WeaponType.Missile })
            }
        };

        // Act
        await battleManager.ApplyEventAsync(updateEvent);

        // Assert
        var updatedShip = battleManager.BattleState.Fractions.First().Ships.First();
        updatedShip.Type.ShouldBe(ShipType.Destroyer);
        updatedShip.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public async Task UpdateFractionShipEventHandler_UpdateShipPosition_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
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

        var updateEvent = new UpdateFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = ship.ShipId,
            Name = "Test Ship",
            Type = ShipType.Corvette,
            PositionX = 200,
            PositionY = 300,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser })
            }
        };

        // Act
        await battleManager.ApplyEventAsync(updateEvent);

        // Assert
        var updatedShip = battleManager.BattleState.Fractions.First().Ships.First();
        updatedShip.Position.X.ShouldBe(200);
        updatedShip.Position.Y.ShouldBe(300);
    }

    [Fact]
    public async Task UpdateFractionShipEventHandler_WithInvalidShipId_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        var battleManager = new BattleManager(battleState);

        var updateEvent = new UpdateFractionShipEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            ShipId = Guid.NewGuid(),
            Name = "Test Ship",
            Type = ShipType.Corvette,
            PositionX = 100,
            PositionY = 100,
            Modules = new List<Module>
            {
                new Module(new List<WeaponType> { WeaponType.Laser })
            }
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(updateEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<InvalidOperationException>();
    }
}
