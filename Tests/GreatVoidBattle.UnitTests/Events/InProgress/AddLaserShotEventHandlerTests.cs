using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events.InProgress;

public class AddLaserShotEventHandlerTests
{
    [Fact]
    public async Task AddLaserShotEventHandler_AddSingleShot_Success()
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
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 150,
            positionY: 150,
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

        var laserEvent = new AddLaserShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act
        await battleManager.ApplyEventAsync(laserEvent);

        // Assert
        battleManager.BattleState.LaserShots.Count.ShouldBe(1);
        var shot = battleManager.BattleState.LaserShots.First();
        shot.ShipId.ShouldBe(attackerShip.ShipId);
        shot.TargetId.ShouldBe(targetShip.ShipId);
        attackerShip.NumberOfLasersFiredPerTurn.ShouldBe(1);
    }

    [Fact]
    public async Task AddLaserShotEventHandler_MultipleShots_Success()
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
                ModuleState.Create(new List<SystemSlot> 
                { 
                    SystemSlot.Create(WeaponType.Laser),
                    SystemSlot.Create(WeaponType.Laser)
                })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 150,
            positionY: 150,
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

        var laserEvent1 = new AddLaserShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        var laserEvent2 = new AddLaserShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act
        await battleManager.ApplyEventAsync(laserEvent1);
        await battleManager.ApplyEventAsync(laserEvent2);

        // Assert
        battleManager.BattleState.LaserShots.Count.ShouldBe(2);
        attackerShip.NumberOfLasersFiredPerTurn.ShouldBe(2);
    }

    [Fact]
    public async Task AddLaserShotEventHandler_WhenBattleNotInProgress_ThrowsException()
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
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) })
            },
            battleLog: battleState.BattleLog
        );

        var targetShip = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target",
            type: ShipType.Corvette,
            positionX: 150,
            positionY: 150,
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

        var laserEvent = new AddLaserShotEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction1.FractionId,
            ShipId = attackerShip.ShipId,
            TargetFractionId = fraction2.FractionId,
            TargetShipId = targetShip.ShipId
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(laserEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}
