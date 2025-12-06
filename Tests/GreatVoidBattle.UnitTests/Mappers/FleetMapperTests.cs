using GreatVoidBattle.Application.Mappers;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Domains.GameState;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Mappers;

public class FleetMapperTests
{
    private static FractionShipTemplate CreateTestShip(
        string name = "Test Ship",
        ShipType type = ShipType.Corvette,
        ShipCategory category = ShipCategory.Combat,
        string? stationedInSystemId = null)
    {
        var ship = FractionShipTemplate.CreateDefault(name, type, category);
        ship.StationedInSystemId = stationedInSystemId;
        return ship;
    }

    private static PlanetarySystem CreateTestSystem(string name = "Test System", string description = "Test Description")
    {
        return new PlanetarySystem
        {
            Name = name,
            Description = description
        };
    }

    #region ToDto Tests (Ship)

    [Fact]
    public void ToDto_Ship_ShouldMapAllBasicProperties()
    {
        // Arrange
        var ship = CreateTestShip("ISS Titan", ShipType.Battleship, ShipCategory.Combat);

        // Act
        var dto = FleetMapper.ToDto(ship);

        // Assert
        dto.Id.ShouldBe(ship.Id);
        dto.Name.ShouldBe("ISS Titan");
        dto.Type.ShouldBe("Battleship");
        dto.Category.ShouldBe("Combat");
        dto.Speed.ShouldBe(ship.Speed);
        dto.HitPoints.ShouldBe(ship.HitPoints);
        dto.Shields.ShouldBe(ship.Shields);
        dto.Armor.ShouldBe(ship.Armor);
        dto.NumberOfModules.ShouldBe(ship.NumberOfModules);
    }

    [Fact]
    public void ToDto_Ship_ShouldMapModules()
    {
        // Arrange
        var ship = CreateTestShip("Test Ship", ShipType.Destroyer);

        // Act
        var dto = FleetMapper.ToDto(ship);

        // Assert
        dto.Modules.ShouldNotBeNull();
        dto.Modules.Count.ShouldBe(ship.Modules.Count);
    }

    [Fact]
    public void ToDto_Ship_ShouldMapModuleWeaponTypes()
    {
        // Arrange
        var ship = CreateTestShip("Test Ship", ShipType.Corvette);

        // Act
        var dto = FleetMapper.ToDto(ship);

        // Assert
        dto.Modules.First().WeaponTypes.ShouldContain("Missile");
        dto.Modules.First().WeaponTypes.ShouldContain("Laser");
        dto.Modules.First().WeaponTypes.ShouldContain("PointDefense");
    }

    [Fact]
    public void ToDto_Ship_WithStationedSystem_ShouldMapSystemName()
    {
        // Arrange
        var system = CreateTestSystem("Alpha Centauri");
        var ship = CreateTestShip("Stationed Ship", stationedInSystemId: system.Id);
        var systems = new List<PlanetarySystem> { system };

        // Act
        var dto = FleetMapper.ToDto(ship, systems);

        // Assert
        dto.StationedInSystemId.ShouldBe(system.Id);
        dto.StationedInSystemName.ShouldBe("Alpha Centauri");
    }

    [Fact]
    public void ToDto_Ship_WithoutStationedSystem_ShouldHaveNullSystemName()
    {
        // Arrange
        var ship = CreateTestShip("Free Ship");

        // Act
        var dto = FleetMapper.ToDto(ship);

        // Assert
        dto.StationedInSystemId.ShouldBeNull();
        dto.StationedInSystemName.ShouldBeNull();
    }

    [Theory]
    [InlineData(ShipCategory.Combat, "Combat")]
    [InlineData(ShipCategory.Research, "Research")]
    [InlineData(ShipCategory.OrbitalStation, "OrbitalStation")]
    public void ToDto_Ship_ShouldMapCategoryCorrectly(ShipCategory category, string expectedString)
    {
        // Arrange
        var ship = CreateTestShip("Category Test Ship", ShipType.Corvette, category);

        // Act
        var dto = FleetMapper.ToDto(ship);

        // Assert
        dto.Category.ShouldBe(expectedString);
    }

    #endregion

    #region ToDtoList Tests (Ships)

    [Fact]
    public void ToDtoList_Ships_ShouldMapAllShips()
    {
        // Arrange
        var ships = new List<FractionShipTemplate>
        {
            CreateTestShip("Ship 1"),
            CreateTestShip("Ship 2"),
            CreateTestShip("Ship 3")
        };

        // Act
        var dtos = FleetMapper.ToDtoList(ships);

        // Assert
        dtos.Count.ShouldBe(3);
        dtos.Select(d => d.Name).ShouldBe(new[] { "Ship 1", "Ship 2", "Ship 3" });
    }

    [Fact]
    public void ToDtoList_Ships_WithSystems_ShouldMapSystemNames()
    {
        // Arrange
        var system = CreateTestSystem("Home System");
        var ships = new List<FractionShipTemplate>
        {
            CreateTestShip("Stationed Ship", stationedInSystemId: system.Id),
            CreateTestShip("Free Ship")
        };
        var systems = new List<PlanetarySystem> { system };

        // Act
        var dtos = FleetMapper.ToDtoList(ships, systems);

        // Assert
        dtos[0].StationedInSystemName.ShouldBe("Home System");
        dtos[1].StationedInSystemName.ShouldBeNull();
    }

    [Fact]
    public void ToDtoList_Ships_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var ships = new List<FractionShipTemplate>();

        // Act
        var dtos = FleetMapper.ToDtoList(ships);

        // Assert
        dtos.ShouldNotBeNull();
        dtos.ShouldBeEmpty();
    }

    #endregion

    #region ToDto Tests (PlanetarySystem)

    [Fact]
    public void ToDto_System_ShouldMapBasicProperties()
    {
        // Arrange
        var system = CreateTestSystem("Alpha System", "A distant star system");

        // Act
        var dto = FleetMapper.ToDto(system);

        // Assert
        dto.Id.ShouldBe(system.Id);
        dto.Name.ShouldBe("Alpha System");
        dto.Description.ShouldBe("A distant star system");
    }

    [Fact]
    public void ToDto_System_ShouldMapStationedShipIds()
    {
        // Arrange
        var system = CreateTestSystem();
        system.StationedShipIds.Add("ship-1");
        system.StationedShipIds.Add("ship-2");

        // Act
        var dto = FleetMapper.ToDto(system);

        // Assert
        dto.StationedShipIds.Count.ShouldBe(2);
        dto.StationedShipIds.ShouldContain("ship-1");
        dto.StationedShipIds.ShouldContain("ship-2");
    }

    [Fact]
    public void ToDto_System_WithShips_ShouldMapStationedShips()
    {
        // Arrange
        var system = CreateTestSystem("Home System");
        var ship1 = CreateTestShip("Ship 1", stationedInSystemId: system.Id);
        var ship2 = CreateTestShip("Ship 2", stationedInSystemId: system.Id);
        var ships = new List<FractionShipTemplate> { ship1, ship2 };

        // Act
        var dto = FleetMapper.ToDto(system, ships);

        // Assert
        dto.StationedShips.Count.ShouldBe(2);
        dto.StationedShips.Select(s => s.Name).ShouldBe(new[] { "Ship 1", "Ship 2" });
    }

    [Fact]
    public void ToDto_System_WithoutShips_ShouldHaveEmptyStationedShips()
    {
        // Arrange
        var system = CreateTestSystem();

        // Act
        var dto = FleetMapper.ToDto(system);

        // Assert
        dto.StationedShips.ShouldNotBeNull();
        dto.StationedShips.ShouldBeEmpty();
    }

    #endregion

    #region ToDtoList Tests (Systems)

    [Fact]
    public void ToDtoList_Systems_ShouldMapAllSystems()
    {
        // Arrange
        var systems = new List<PlanetarySystem>
        {
            CreateTestSystem("System 1"),
            CreateTestSystem("System 2"),
            CreateTestSystem("System 3")
        };

        // Act
        var dtos = FleetMapper.ToDtoList(systems);

        // Assert
        dtos.Count.ShouldBe(3);
        dtos.Select(d => d.Name).ShouldBe(new[] { "System 1", "System 2", "System 3" });
    }

    [Fact]
    public void ToDtoList_Systems_WithEmptyList_ShouldReturnEmptyList()
    {
        // Arrange
        var systems = new List<PlanetarySystem>();

        // Act
        var dtos = FleetMapper.ToDtoList(systems);

        // Assert
        dtos.ShouldNotBeNull();
        dtos.ShouldBeEmpty();
    }

    #endregion

    #region ToFractionFleetDto Tests

    [Fact]
    public void ToFractionFleetDto_ShouldMapFractionId()
    {
        // Arrange
        var state = new FractionGameState { FractionId = "test-fraction" };

        // Act
        var dto = FleetMapper.ToFractionFleetDto(state);

        // Assert
        dto.FractionId.ShouldBe("test-fraction");
    }

    [Fact]
    public void ToFractionFleetDto_ShouldMapAllFleetShips()
    {
        // Arrange
        var state = new FractionGameState { FractionId = "test-fraction" };
        state.Fleet.Add(CreateTestShip("Ship 1"));
        state.Fleet.Add(CreateTestShip("Ship 2"));

        // Act
        var dto = FleetMapper.ToFractionFleetDto(state);

        // Assert
        dto.Fleet.Count.ShouldBe(2);
    }

    [Fact]
    public void ToFractionFleetDto_ShouldMapAllPlanetarySystems()
    {
        // Arrange
        var state = new FractionGameState { FractionId = "test-fraction" };
        state.PlanetarySystems.Add(CreateTestSystem("System 1"));
        state.PlanetarySystems.Add(CreateTestSystem("System 2"));

        // Act
        var dto = FleetMapper.ToFractionFleetDto(state);

        // Assert
        dto.PlanetarySystems.Count.ShouldBe(2);
    }

    [Fact]
    public void ToFractionFleetDto_ShouldIdentifyUnassignedShips()
    {
        // Arrange
        var state = new FractionGameState { FractionId = "test-fraction" };
        var system = CreateTestSystem("Home System");
        state.PlanetarySystems.Add(system);
        
        state.Fleet.Add(CreateTestShip("Stationed Ship", stationedInSystemId: system.Id));
        state.Fleet.Add(CreateTestShip("Free Ship 1"));
        state.Fleet.Add(CreateTestShip("Free Ship 2"));

        // Act
        var dto = FleetMapper.ToFractionFleetDto(state);

        // Assert
        dto.UnassignedShips.Count.ShouldBe(2);
        dto.UnassignedShips.Select(s => s.Name).ShouldContain("Free Ship 1");
        dto.UnassignedShips.Select(s => s.Name).ShouldContain("Free Ship 2");
    }

    [Fact]
    public void ToFractionFleetDto_WithEmptyState_ShouldReturnEmptyLists()
    {
        // Arrange
        var state = new FractionGameState { FractionId = "test-fraction" };

        // Act
        var dto = FleetMapper.ToFractionFleetDto(state);

        // Assert
        dto.Fleet.ShouldBeEmpty();
        dto.PlanetarySystems.ShouldBeEmpty();
        dto.UnassignedShips.ShouldBeEmpty();
    }

    #endregion
}

