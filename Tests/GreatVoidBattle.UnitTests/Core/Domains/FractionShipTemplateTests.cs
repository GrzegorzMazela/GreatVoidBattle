using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Domains.GameState;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class FractionShipTemplateTests
{
    #region CreateDefault Tests

    [Fact]
    public void CreateDefault_Corvette_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Corvette", ShipType.Corvette);

        // Assert
        ship.Name.ShouldBe("Test Corvette");
        ship.Type.ShouldBe(ShipType.Corvette);
        ship.Category.ShouldBe(ShipCategory.Combat);
        ship.Speed.ShouldBe(10);
        ship.HitPoints.ShouldBe(50);
        ship.Shields.ShouldBe(25);
        ship.Armor.ShouldBe(25);
        ship.NumberOfModules.ShouldBe(1);
        ship.Modules.Count.ShouldBe(1);
    }

    [Fact]
    public void CreateDefault_Destroyer_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Destroyer", ShipType.Destroyer);

        // Assert
        ship.Speed.ShouldBe(8);
        ship.HitPoints.ShouldBe(100);
        ship.Shields.ShouldBe(50);
        ship.Armor.ShouldBe(50);
        ship.NumberOfModules.ShouldBe(2);
        ship.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public void CreateDefault_Cruiser_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Cruiser", ShipType.Cruiser);

        // Assert
        ship.Speed.ShouldBe(6);
        ship.HitPoints.ShouldBe(200);
        ship.Shields.ShouldBe(100);
        ship.Armor.ShouldBe(100);
        ship.NumberOfModules.ShouldBe(4);
        ship.Modules.Count.ShouldBe(4);
    }

    [Fact]
    public void CreateDefault_Battleship_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Battleship", ShipType.Battleship);

        // Assert
        ship.Speed.ShouldBe(5);
        ship.HitPoints.ShouldBe(400);
        ship.Shields.ShouldBe(200);
        ship.Armor.ShouldBe(200);
        ship.NumberOfModules.ShouldBe(8);
        ship.Modules.Count.ShouldBe(8);
    }

    [Fact]
    public void CreateDefault_SuperBattleship_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test SuperBattleship", ShipType.SuperBattleship);

        // Assert
        ship.Speed.ShouldBe(5);
        ship.HitPoints.ShouldBe(600);
        ship.Shields.ShouldBe(300);
        ship.Armor.ShouldBe(300);
        ship.NumberOfModules.ShouldBe(12);
        ship.Modules.Count.ShouldBe(12);
    }

    [Fact]
    public void CreateDefault_OrbitalFort_ShouldHaveCorrectStats()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test OrbitalFort", ShipType.OrbitalFort);

        // Assert
        ship.Speed.ShouldBe(0); // Orbital forts don't move
        ship.HitPoints.ShouldBe(100);
        ship.Shields.ShouldBe(50);
        ship.Armor.ShouldBe(50);
        ship.NumberOfModules.ShouldBe(2);
        ship.Modules.Count.ShouldBe(2);
    }

    [Fact]
    public void CreateDefault_WithCategory_ShouldSetCategory()
    {
        // Act
        var combatShip = FractionShipTemplate.CreateDefault("Combat Ship", ShipType.Corvette, ShipCategory.Combat);
        var researchShip = FractionShipTemplate.CreateDefault("Research Ship", ShipType.Corvette, ShipCategory.Research);
        var station = FractionShipTemplate.CreateDefault("Station", ShipType.OrbitalFort, ShipCategory.OrbitalStation);

        // Assert
        combatShip.Category.ShouldBe(ShipCategory.Combat);
        researchShip.Category.ShouldBe(ShipCategory.Research);
        station.Category.ShouldBe(ShipCategory.OrbitalStation);
    }

    [Fact]
    public void CreateDefault_ShouldGenerateUniqueId()
    {
        // Act
        var ship1 = FractionShipTemplate.CreateDefault("Ship 1", ShipType.Corvette);
        var ship2 = FractionShipTemplate.CreateDefault("Ship 2", ShipType.Corvette);

        // Assert
        ship1.Id.ShouldNotBeNullOrEmpty();
        ship2.Id.ShouldNotBeNullOrEmpty();
        ship1.Id.ShouldNotBe(ship2.Id);
    }

    [Fact]
    public void CreateDefault_ModulesShouldHaveDefaultWeapons()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Ship", ShipType.Destroyer);

        // Assert
        ship.Modules.ShouldAllBe(m => 
            m.WeaponTypes.Count == 3 &&
            m.WeaponTypes.Contains(WeaponType.Missile) &&
            m.WeaponTypes.Contains(WeaponType.Laser) &&
            m.WeaponTypes.Contains(WeaponType.PointDefense)
        );
    }

    #endregion

    #region StationedInSystemId Tests

    [Fact]
    public void NewShip_ShouldNotBeStationedAnywhere()
    {
        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Ship", ShipType.Corvette);

        // Assert
        ship.StationedInSystemId.ShouldBeNull();
    }

    [Fact]
    public void Ship_CanBeAssignedToSystem()
    {
        // Arrange
        var ship = FractionShipTemplate.CreateDefault("Test Ship", ShipType.Corvette);
        var systemId = "system-123";

        // Act
        ship.StationedInSystemId = systemId;

        // Assert
        ship.StationedInSystemId.ShouldBe(systemId);
    }

    #endregion

    #region Timestamps Tests

    [Fact]
    public void NewShip_ShouldHaveCreatedAtSet()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Ship", ShipType.Corvette);

        // Assert
        ship.CreatedAt.ShouldBeGreaterThan(beforeCreation);
        ship.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    [Fact]
    public void NewShip_ShouldHaveUpdatedAtSet()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var ship = FractionShipTemplate.CreateDefault("Test Ship", ShipType.Corvette);

        // Assert
        ship.UpdatedAt.ShouldBeGreaterThan(beforeCreation);
    }

    #endregion
}

