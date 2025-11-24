using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events.InProgress;

public class AddMissileShotEventHandlerTests
{
    [Fact]
    public async Task AddMissileShotEventHandler_AddSingleShot_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var attackerShip = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker",
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
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Missile) })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 120,
            positionY: 120,
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

        battleState.AddFractionShip(fraction1.FractionId, attackerShip);
        battleState.AddFractionShip(fraction2.FractionId, targetShip);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var missileEvent = new AddMissileShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act
        await battleManager.ApplyEventAsync(missileEvent);

        // Assert
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(1);
        var missile = battleManager.BattleState.MissileMovementPaths.First();
        missile.ShipId.ShouldBe(attackerShip.ShipId);
        missile.TargetId.ShouldBe(targetShip.ShipId);
        attackerShip.NumberOfMissilesFiredPerTurn.ShouldBe(1);
    }

    [Fact]
    public async Task AddMissileShotEventHandler_MultipleShots_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var attackerShip = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker",
            type: ShipType.Destroyer,
            positionX: 100,
            positionY: 100,
            speed: 8,
            hitPoints: 200,
            shields: 100,
            armor: 50,
            numberOfModules: 2,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> 
                { 
                    SystemSlot.Create(WeaponType.Missile),
                    SystemSlot.Create(WeaponType.Missile)
                }),
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 120,
            positionY: 120,
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

        battleState.AddFractionShip(fraction1.FractionId, attackerShip);
        battleState.AddFractionShip(fraction2.FractionId, targetShip);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var missileEvent1 = new AddMissileShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        var missileEvent2 = new AddMissileShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act
        await battleManager.ApplyEventAsync(missileEvent1);
        await battleManager.ApplyEventAsync(missileEvent2);

        // Assert
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(2);
        attackerShip.NumberOfMissilesFiredPerTurn.ShouldBe(2);
    }

    [Fact]
    public async Task AddMissileShotEventHandler_WhenBattleNotInProgress_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var attackerShip = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker",
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
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Missile) })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 120,
            positionY: 120,
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

        battleState.AddFractionShip(fraction1.FractionId, attackerShip);
        battleState.AddFractionShip(fraction2.FractionId, targetShip);
        var battleManager = new BattleManager(battleState);

        var missileEvent = new AddMissileShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(missileEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}
