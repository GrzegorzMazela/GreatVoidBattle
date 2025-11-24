using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;
using Xunit;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class BattleStateTests
{
    #region Preparation Phase Tests

    [Fact]
    public void CreateNew_ShouldCreateBattleWithCorrectProperties()
    {
        // Arrange
        var battleName = "Test Battle";
        var width = 1000;
        var height = 800;

        // Act
        var battleState = BattleState.CreateNew(battleName, width, height);

        // Assert
        battleState.BattleId.ShouldNotBe(Guid.Empty);
        battleState.BattleName.ShouldBe(battleName);
        battleState.Width.ShouldBe(width);
        battleState.Height.ShouldBe(height);
        battleState.TurnNumber.ShouldBe(0);
        battleState.BattleStatus.ShouldBe(BattleStatus.Preparation);
        battleState.Fractions.Count.ShouldBe(0);
        battleState.IsDeleted.ShouldBeFalse();
        battleState.BattleLog.ShouldNotBeNull();
    }

    [Fact]
    public void AddFraction_ShouldAddFractionSuccessfully()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");

        // Act
        battleState.AddFraction(fraction);

        // Assert
        battleState.Fractions.Count.ShouldBe(1);
        battleState.Fractions.First().FractionId.ShouldBe(fraction.FractionId);
        battleState.Fractions.First().FractionName.ShouldBe("Fraction 1");
    }

    [Fact]
    public void AddFraction_WithDuplicateId_ShouldThrowException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);

        // Act & Assert
        Should.Throw<InvalidOperationException>(() => battleState.AddFraction(fraction))
            .Message.ShouldContain("already exists");
    }

    [Fact]
    public void AddFractionShip_ShouldAddShipToFraction()
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

        // Act
        battleState.AddFractionShip(fraction.FractionId, ship);

        // Assert
        battleState.Fractions.First().Ships.Count.ShouldBe(1);
        battleState.Fractions.First().Ships.First().ShipId.ShouldBe(ship.ShipId);
    }

    [Fact]
    public void AddFractionShip_WithInvalidFractionId_ShouldThrowException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var invalidFractionId = Guid.NewGuid();

        var ship = ShipState.Create(
            fractionId: invalidFractionId,
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

        // Act & Assert
        Should.Throw<NullReferenceException>(() => battleState.AddFractionShip(invalidFractionId, ship));
    }

    [Fact]
    public void SetNewShipPosition_ShouldUpdateShipPosition()
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

        var newX = 200.0;
        var newY = 300.0;

        // Act
        battleState.SetNewShipPosition(fraction.FractionId, ship.ShipId, newX, newY);

        // Assert
        var updatedShip = battleState.Fractions.First().Ships.First();
        updatedShip.Position.X.ShouldBe(newX);
        updatedShip.Position.Y.ShouldBe(newY);
    }

    [Fact]
    public void StartBattle_ShouldChangeBattleStatusToInProgress()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        battleState.BattleStatus.ShouldBe(BattleStatus.Preparation);

        // Act
        battleState.StartBattle();

        // Assert
        battleState.BattleStatus.ShouldBe(BattleStatus.InProgress);
        battleState.TurnNumber.ShouldBe(1);
    }

    #endregion

    #region InProgress Phase Tests

    [Fact]
    public void GetShip_WithValidId_ShouldReturnShip()
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

        // Act
        var foundShip = battleState.GetShip(ship.ShipId);

        // Assert
        foundShip.ShouldNotBeNull();
        foundShip!.ShipId.ShouldBe(ship.ShipId);
        foundShip.Name.ShouldBe("Test Ship");
    }

    [Fact]
    public void GetShip_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var invalidShipId = Guid.NewGuid();

        // Act
        var foundShip = battleState.GetShip(invalidShipId);

        // Assert
        foundShip.ShouldBeNull();
    }

    [Fact]
    public void AddShipMove_ShouldAddMovementPath()
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

        var targetPosition = new Position(200, 200);

        // Act
        battleState.AddShipMove(fraction.FractionId, ship.ShipId, targetPosition);

        // Assert
        battleState.ShipMovementPaths.Count.ShouldBe(1);
        battleState.ShipMovementPaths.First().ShipId.ShouldBe(ship.ShipId);
    }

    [Fact]
    public void AddLaserShot_ShouldAddLaserToCollection()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var ship1 = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker Ship",
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

        var ship2 = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target Ship",
            type: ShipType.Corvette,
            positionX: 200,
            positionY: 200,
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

        battleState.AddFractionShip(fraction1.FractionId, ship1);
        battleState.AddFractionShip(fraction2.FractionId, ship2);
        battleState.StartBattle();

        // Act
        battleState.AddLaserShot(fraction1.FractionId, ship1.ShipId, fraction2.FractionId, ship2.ShipId);

        // Assert
        battleState.LaserShots.Count.ShouldBe(1);
        battleState.LaserShots.First().ShipId.ShouldBe(ship1.ShipId);
        battleState.LaserShots.First().TargetId.ShouldBe(ship2.ShipId);
    }

    [Fact]
    public void AddMissileShot_ShouldAddMissilePathToCollection()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        var fraction1 = FractionState.CreateNew("Fraction 1", "Player 1", "#FF0000");
        var fraction2 = FractionState.CreateNew("Fraction 2", "Player 2", "#00FF00");
        battleState.AddFraction(fraction1);
        battleState.AddFraction(fraction2);

        var ship1 = ShipState.Create(
            fractionId: fraction1.FractionId,
            name: "Attacker Ship",
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

        var ship2 = ShipState.Create(
            fractionId: fraction2.FractionId,
            name: "Target Ship",
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

        battleState.AddFractionShip(fraction1.FractionId, ship1);
        battleState.AddFractionShip(fraction2.FractionId, ship2);
        battleState.StartBattle();

        // Act
        battleState.AddMissileShot(fraction1.FractionId, ship1.ShipId, fraction2.FractionId, ship2.ShipId);

        // Assert
        battleState.MissileMovementPaths.Count.ShouldBe(1);
        battleState.MissileMovementPaths.First().ShipId.ShouldBe(ship1.ShipId);
        battleState.MissileMovementPaths.First().TargetId.ShouldBe(ship2.ShipId);
        ship1.NumberOfMissilesFiredPerTurn.ShouldBe(1);
    }

    [Fact]
    public void EndOfTurn_ShouldIncrementTurnNumber()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
        battleState.StartBattle();
        var initialTurn = battleState.TurnNumber;

        // Act
        battleState.EndOfTurn();

        // Assert
        battleState.TurnNumber.ShouldBe(initialTurn + 1);
    }

    [Fact]
    public void EndOfTurn_ShouldResetShipFireCounters()
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

        ship.FireLaser();
        ship.FireMissile();
        ship.NumberOfLasersFiredPerTurn.ShouldBe(1);
        ship.NumberOfMissilesFiredPerTurn.ShouldBe(1);

        // Act
        battleState.EndOfTurn();

        // Assert
        ship.NumberOfLasersFiredPerTurn.ShouldBe(0);
        ship.NumberOfMissilesFiredPerTurn.ShouldBe(0);
    }

    [Fact]
    public void UpdateFractionShip_ShouldUpdateExistingShip()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 1000, 800);
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

        var updatedShip = ShipState.Create(
            fractionId: fraction.FractionId,
            name: "Updated Ship",
            type: ShipType.Destroyer,
            positionX: 200,
            positionY: 200,
            speed: 8,
            hitPoints: 200,
            shields: 100,
            armor: 50,
            numberOfModules: 2,
            modules: new List<ModuleState>
            {
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Laser) }),
                ModuleState.Create(new List<SystemSlot> { SystemSlot.Create(WeaponType.Missile) })
            },
            battleLog: battleState.BattleLog
        );

        // Use reflection to set the same ShipId
        var shipIdProperty = typeof(ShipState).GetProperty("ShipId");
        shipIdProperty!.SetValue(updatedShip, ship.ShipId);

        // Act
        battleState.UpdateFractionShip(fraction.FractionId, updatedShip);

        // Assert
        var resultShip = battleState.Fractions.First().Ships.First();
        resultShip.ShipId.ShouldBe(ship.ShipId);
        resultShip.Name.ShouldBe("Updated Ship");
        resultShip.Type.ShouldBe(ShipType.Destroyer);
    }

    #endregion
}
