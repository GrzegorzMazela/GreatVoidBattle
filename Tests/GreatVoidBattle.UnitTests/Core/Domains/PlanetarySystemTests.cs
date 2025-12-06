using GreatVoidBattle.Core.Domains.GameState;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains;

public class PlanetarySystemTests
{
    #region Creation Tests

    [Fact]
    public void NewPlanetarySystem_ShouldHaveEmptyProperties()
    {
        // Act
        var system = new PlanetarySystem();

        // Assert
        system.Name.ShouldBe(string.Empty);
        system.Description.ShouldBe(string.Empty);
        system.StationedShipIds.ShouldNotBeNull();
        system.StationedShipIds.ShouldBeEmpty();
    }

    [Fact]
    public void NewPlanetarySystem_ShouldHaveUniqueId()
    {
        // Act
        var system1 = new PlanetarySystem();
        var system2 = new PlanetarySystem();

        // Assert
        system1.Id.ShouldNotBeNullOrEmpty();
        system2.Id.ShouldNotBeNullOrEmpty();
        system1.Id.ShouldNotBe(system2.Id);
    }

    [Fact]
    public void PlanetarySystem_ShouldAllowSettingNameAndDescription()
    {
        // Arrange
        var system = new PlanetarySystem
        {
            Name = "Alpha Centauri",
            Description = "A nearby star system with multiple planets"
        };

        // Assert
        system.Name.ShouldBe("Alpha Centauri");
        system.Description.ShouldBe("A nearby star system with multiple planets");
    }

    #endregion

    #region StationedShipIds Tests

    [Fact]
    public void PlanetarySystem_CanAddStationedShips()
    {
        // Arrange
        var system = new PlanetarySystem { Name = "Test System" };
        var shipId1 = "ship-1";
        var shipId2 = "ship-2";

        // Act
        system.StationedShipIds.Add(shipId1);
        system.StationedShipIds.Add(shipId2);

        // Assert
        system.StationedShipIds.Count.ShouldBe(2);
        system.StationedShipIds.ShouldContain(shipId1);
        system.StationedShipIds.ShouldContain(shipId2);
    }

    [Fact]
    public void PlanetarySystem_CanRemoveStationedShips()
    {
        // Arrange
        var system = new PlanetarySystem { Name = "Test System" };
        system.StationedShipIds.Add("ship-1");
        system.StationedShipIds.Add("ship-2");
        system.StationedShipIds.Add("ship-3");

        // Act
        system.StationedShipIds.Remove("ship-2");

        // Assert
        system.StationedShipIds.Count.ShouldBe(2);
        system.StationedShipIds.ShouldNotContain("ship-2");
    }

    #endregion

    #region Timestamps Tests

    [Fact]
    public void NewPlanetarySystem_ShouldHaveTimestampsSet()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var system = new PlanetarySystem();

        // Assert
        system.CreatedAt.ShouldBeGreaterThan(beforeCreation);
        system.CreatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
        system.UpdatedAt.ShouldBeGreaterThan(beforeCreation);
        system.UpdatedAt.ShouldBeLessThanOrEqualTo(DateTime.UtcNow);
    }

    #endregion
}

