using GreatVoidBattle.Application.Events.InProgress;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events.InProgress;

public class EndOfTurnEventHandlerTests
{
    [Fact]
    public async Task EndOfTurnEventHandler_IncrementsTurnNumber_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var initialTurn = battleManager.BattleState.TurnNumber;

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act
        await battleManager.ApplyEventAsync(endTurnEvent);

        // Assert
        battleManager.BattleState.TurnNumber.ShouldBe(initialTurn + 1);
    }

    [Fact]
    public async Task EndOfTurnEventHandler_ResetsShipCounters_Success()
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
                ModuleState.Create(new List<SystemSlot> 
                { 
                    SystemSlot.Create(WeaponType.Laser),
                    SystemSlot.Create(WeaponType.Missile)
                })
            },
            battleLog: battleState.BattleLog
        );
        battleState.AddFractionShip(fraction.FractionId, ship);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        // Fire weapons
        ship.FireLaser();
        ship.FireMissile();

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act
        await battleManager.ApplyEventAsync(endTurnEvent);

        // Assert
        ship.NumberOfLasersFiredPerTurn.ShouldBe(0);
        ship.NumberOfMissilesFiredPerTurn.ShouldBe(0);
    }

    [Fact]
    public async Task EndOfTurnEventHandler_ProcessesLaserShots_Success()
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
        
        var initialHP = targetShip.HitPoints + targetShip.Shields + targetShip.Armor;
        
        battleState.AddLaserShot(fraction1.FractionId, attackerShip.ShipId, 
            fraction2.FractionId, targetShip.ShipId);

        var battleManager = new BattleManager(battleState);

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act
        await battleManager.ApplyEventAsync(endTurnEvent);

        // Assert
        battleManager.BattleState.LaserShots.Count.ShouldBe(0); // Shots should be cleared after processing
        // Target ship should have taken some damage (if laser hit)
        var currentHP = targetShip.HitPoints + targetShip.Shields + targetShip.Armor;
        (currentHP <= initialHP).ShouldBeTrue();
    }

    [Fact]
    public async Task EndOfTurnEventHandler_MultipleTurns_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act - Execute multiple turns
        await battleManager.ApplyEventAsync(endTurnEvent);
        await battleManager.ApplyEventAsync(endTurnEvent);
        await battleManager.ApplyEventAsync(endTurnEvent);

        // Assert
        battleManager.BattleState.TurnNumber.ShouldBe(4); // Started at 1, then +3
    }

    [Fact]
    public async Task EndOfTurnEventHandler_ProcessesMissileShots_Success()
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
            positionX: 110,
            positionY: 110,
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
        
        var initialHP = targetShip.HitPoints + targetShip.Shields + targetShip.Armor;
        
        // Fire missile
        battleState.AddMissileShot(fraction1.FractionId, attackerShip.ShipId, 
            fraction2.FractionId, targetShip.ShipId);

        var battleManager = new BattleManager(battleState);

        // Verify missile path was created
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(1);

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act - Process turns until missile reaches target
        var maxTurns = 50; // Safety limit
        var turnCount = 0;
        
        while (battleManager.BattleState.MissileMovementPaths.Count > 0 && turnCount < maxTurns)
        {
            await battleManager.ApplyEventAsync(endTurnEvent);
            turnCount++;
        }

        // Assert
        // Missiles should have been removed after reaching target
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(0);
        
        // Missile paths should be cleared
        battleManager.BattleState.MissileMovementPaths.ShouldBeEmpty();
        
        // Target ship should have taken some damage (if missile hit)
        var currentHP = targetShip.HitPoints + targetShip.Shields + targetShip.Armor;
        (currentHP <= initialHP).ShouldBeTrue();
        
        // Turn count should be reasonable (not hit max safety limit)
        turnCount.ShouldBeLessThan(maxTurns);
    }

    [Fact]
    public async Task EndOfTurnEventHandler_MultipleMissilesReachTarget_Success()
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
            positionX: 110,
            positionY: 110,
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
        
        // Fire multiple missiles
        battleState.AddMissileShot(fraction1.FractionId, attackerShip.ShipId, 
            fraction2.FractionId, targetShip.ShipId);
        battleState.AddMissileShot(fraction1.FractionId, attackerShip.ShipId, 
            fraction2.FractionId, targetShip.ShipId);

        var battleManager = new BattleManager(battleState);

        // Verify missile paths were created
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(2);

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act - Process turns until all missiles reach target
        var maxTurns = 50;
        var turnCount = 0;
        
        while (battleManager.BattleState.MissileMovementPaths.Count > 0 && turnCount < maxTurns)
        {
            await battleManager.ApplyEventAsync(endTurnEvent);
            turnCount++;
        }

        // Assert
        // All missiles should have been removed
        battleManager.BattleState.MissileMovementPaths.Count.ShouldBe(0);
        battleManager.BattleState.MissileMovementPaths.ShouldBeEmpty();
        
        // Turn count should be reasonable
        turnCount.ShouldBeLessThan(maxTurns);
    }

    [Fact]
    public async Task EndOfTurnEventHandler_WhenBattleNotInProgress_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        var battleManager = new BattleManager(battleState);

        var endTurnEvent = new EndOfTurnEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(endTurnEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}
