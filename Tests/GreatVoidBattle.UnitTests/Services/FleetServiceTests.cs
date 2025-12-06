using GreatVoidBattle.Application.Dto.Fleet;
using GreatVoidBattle.Application.Dto.Ships;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Core.Domains.GameState;
using Moq;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Services;

public class FleetServiceTests
{
    private readonly Mock<IFractionGameStateRepository> _repositoryMock;
    private readonly FleetService _fleetService;
    private readonly string _testFractionId = "test-fraction";

    public FleetServiceTests()
    {
        _repositoryMock = new Mock<IFractionGameStateRepository>();
        _fleetService = new FleetService(_repositoryMock.Object);
    }

    private FractionGameState CreateTestState()
    {
        return new FractionGameState
        {
            Id = "state-1",
            FractionId = _testFractionId
        };
    }

    private void SetupRepositoryWithState(FractionGameState state)
    {
        _repositoryMock
            .Setup(r => r.GetByFractionIdAsync(_testFractionId))
            .ReturnsAsync(state);
        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<FractionGameState>()))
            .Returns(Task.CompletedTask);
    }

    #region Fleet Ships Tests

    [Fact]
    public async Task CreateFleetShipAsync_WithDefaultStats_ShouldCreateShipWithDefaults()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        var dto = new CreateFleetShipDto
        {
            Name = "Test Corvette",
            Type = "Corvette",
            Category = "Combat"
        };

        // Act
        var result = await _fleetService.CreateFleetShipAsync(_testFractionId, dto);

        // Assert
        result.Name.ShouldBe("Test Corvette");
        result.Type.ShouldBe("Corvette");
        result.Category.ShouldBe("Combat");
        result.Speed.ShouldBe(10); // Default corvette speed
        result.HitPoints.ShouldBe(50); // Default corvette HP
        result.Shields.ShouldBe(25);
        result.Armor.ShouldBe(25);
    }

    [Fact]
    public async Task CreateFleetShipAsync_WithCustomStats_ShouldOverrideDefaults()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        var dto = new CreateFleetShipDto
        {
            Name = "Custom Corvette",
            Type = "Corvette",
            Category = "Combat",
            Speed = 15,
            HitPoints = 100,
            Shields = 50,
            Armor = 40
        };

        // Act
        var result = await _fleetService.CreateFleetShipAsync(_testFractionId, dto);

        // Assert
        result.Speed.ShouldBe(15);
        result.HitPoints.ShouldBe(100);
        result.Shields.ShouldBe(50);
        result.Armor.ShouldBe(40);
    }

    [Fact]
    public async Task CreateFleetShipAsync_ShouldAddShipToState()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        var dto = new CreateFleetShipDto
        {
            Name = "New Ship",
            Type = "Destroyer",
            Category = "Combat"
        };

        // Act
        await _fleetService.CreateFleetShipAsync(_testFractionId, dto);

        // Assert
        state.Fleet.Count.ShouldBe(1);
        state.Fleet[0].Name.ShouldBe("New Ship");
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task CreateFleetShipAsync_WithModules_ShouldSetModules()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        var dto = new CreateFleetShipDto
        {
            Name = "Custom Module Ship",
            Type = "Destroyer",
            Category = "Combat",
            Modules = new List<ModuleDto>
            {
                new ModuleDto(new List<string> { "Laser", "Laser", "Missile" }),
                new ModuleDto(new List<string> { "PointDefense", "PointDefense", "PointDefense" })
            }
        };

        // Act
        var result = await _fleetService.CreateFleetShipAsync(_testFractionId, dto);

        // Assert
        result.Modules.Count.ShouldBe(2);
        result.NumberOfModules.ShouldBe(2);
    }

    [Fact]
    public async Task GetFleetShipsAsync_ShouldReturnAllShips()
    {
        // Arrange
        var state = CreateTestState();
        state.Fleet.Add(FractionShipTemplate.CreateDefault("Ship 1", ShipType.Corvette));
        state.Fleet.Add(FractionShipTemplate.CreateDefault("Ship 2", ShipType.Destroyer));
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.GetFleetShipsAsync(_testFractionId);

        // Assert
        result.Count.ShouldBe(2);
        result[0].Name.ShouldBe("Ship 1");
        result[1].Name.ShouldBe("Ship 2");
    }

    [Fact]
    public async Task GetFleetShipAsync_ShouldReturnSpecificShip()
    {
        // Arrange
        var state = CreateTestState();
        var ship = FractionShipTemplate.CreateDefault("Target Ship", ShipType.Cruiser);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.GetFleetShipAsync(_testFractionId, ship.Id);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Target Ship");
        result.Type.ShouldBe("Cruiser");
    }

    [Fact]
    public async Task GetFleetShipAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.GetFleetShipAsync(_testFractionId, "non-existent-id");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task UpdateFleetShipAsync_ShouldUpdateShipProperties()
    {
        // Arrange
        var state = CreateTestState();
        var ship = FractionShipTemplate.CreateDefault("Original Name", ShipType.Corvette);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        var dto = new UpdateFleetShipDto
        {
            Name = "Updated Name",
            Type = "Corvette",
            Category = "Research",
            Speed = 15,
            HitPoints = 100,
            Shields = 50,
            Armor = 40,
            Modules = new List<ModuleDto>
            {
                new ModuleDto(new List<string> { "Laser", "Laser", "Laser" })
            }
        };

        // Act
        var result = await _fleetService.UpdateFleetShipAsync(_testFractionId, ship.Id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Name");
        result.Category.ShouldBe("Research");
        result.Speed.ShouldBe(15);
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task DeleteFleetShipAsync_ShouldRemoveShip()
    {
        // Arrange
        var state = CreateTestState();
        var ship = FractionShipTemplate.CreateDefault("Ship To Delete", ShipType.Corvette);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.DeleteFleetShipAsync(_testFractionId, ship.Id);

        // Assert
        result.ShouldBeTrue();
        state.Fleet.ShouldBeEmpty();
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task DeleteFleetShipAsync_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.DeleteFleetShipAsync(_testFractionId, "non-existent-id");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Planetary Systems Tests

    [Fact]
    public async Task CreatePlanetarySystemAsync_ShouldCreateSystem()
    {
        // Arrange
        var state = CreateTestState();
        SetupRepositoryWithState(state);

        var dto = new CreatePlanetarySystemDto
        {
            Name = "Alpha Centauri",
            Description = "A nearby star system"
        };

        // Act
        var result = await _fleetService.CreatePlanetarySystemAsync(_testFractionId, dto);

        // Assert
        result.Name.ShouldBe("Alpha Centauri");
        result.Description.ShouldBe("A nearby star system");
        result.Id.ShouldNotBeNullOrEmpty();
        state.PlanetarySystems.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetPlanetarySystemsAsync_ShouldReturnAllSystems()
    {
        // Arrange
        var state = CreateTestState();
        state.PlanetarySystems.Add(new PlanetarySystem { Name = "System 1" });
        state.PlanetarySystems.Add(new PlanetarySystem { Name = "System 2" });
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.GetPlanetarySystemsAsync(_testFractionId);

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task UpdatePlanetarySystemAsync_ShouldUpdateSystem()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "Original", Description = "Original Desc" };
        state.PlanetarySystems.Add(system);
        SetupRepositoryWithState(state);

        var dto = new UpdatePlanetarySystemDto
        {
            Name = "Updated",
            Description = "Updated Desc"
        };

        // Act
        var result = await _fleetService.UpdatePlanetarySystemAsync(_testFractionId, system.Id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated");
        result.Description.ShouldBe("Updated Desc");
    }

    [Fact]
    public async Task DeletePlanetarySystemAsync_ShouldRemoveSystem()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "System To Delete" };
        state.PlanetarySystems.Add(system);
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.DeletePlanetarySystemAsync(_testFractionId, system.Id);

        // Assert
        result.ShouldBeTrue();
        state.PlanetarySystems.ShouldBeEmpty();
    }

    [Fact]
    public async Task DeletePlanetarySystemAsync_ShouldUnassignShips()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "System To Delete" };
        var ship = FractionShipTemplate.CreateDefault("Stationed Ship", ShipType.Corvette);
        ship.StationedInSystemId = system.Id;
        state.PlanetarySystems.Add(system);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        // Act
        await _fleetService.DeletePlanetarySystemAsync(_testFractionId, system.Id);

        // Assert
        ship.StationedInSystemId.ShouldBeNull();
    }

    #endregion

    #region Ship Assignment Tests

    [Fact]
    public async Task AssignShipToSystemAsync_ShouldAssignShip()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "Home System" };
        var ship = FractionShipTemplate.CreateDefault("Unassigned Ship", ShipType.Corvette);
        state.PlanetarySystems.Add(system);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        var dto = new AssignShipToSystemDto { SystemId = system.Id };

        // Act
        var result = await _fleetService.AssignShipToSystemAsync(_testFractionId, ship.Id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.StationedInSystemId.ShouldBe(system.Id);
        ship.StationedInSystemId.ShouldBe(system.Id);
        system.StationedShipIds.ShouldContain(ship.Id);
    }

    [Fact]
    public async Task AssignShipToSystemAsync_WithNullSystemId_ShouldUnassignShip()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "Home System" };
        var ship = FractionShipTemplate.CreateDefault("Assigned Ship", ShipType.Corvette);
        ship.StationedInSystemId = system.Id;
        system.StationedShipIds.Add(ship.Id);
        state.PlanetarySystems.Add(system);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        var dto = new AssignShipToSystemDto { SystemId = null };

        // Act
        var result = await _fleetService.AssignShipToSystemAsync(_testFractionId, ship.Id, dto);

        // Assert
        result.ShouldNotBeNull();
        result.StationedInSystemId.ShouldBeNull();
        ship.StationedInSystemId.ShouldBeNull();
        system.StationedShipIds.ShouldNotContain(ship.Id);
    }

    [Fact]
    public async Task AssignShipToSystemAsync_ShouldMoveShipBetweenSystems()
    {
        // Arrange
        var state = CreateTestState();
        var system1 = new PlanetarySystem { Name = "System 1" };
        var system2 = new PlanetarySystem { Name = "System 2" };
        var ship = FractionShipTemplate.CreateDefault("Ship", ShipType.Corvette);
        ship.StationedInSystemId = system1.Id;
        system1.StationedShipIds.Add(ship.Id);
        state.PlanetarySystems.Add(system1);
        state.PlanetarySystems.Add(system2);
        state.Fleet.Add(ship);
        SetupRepositoryWithState(state);

        var dto = new AssignShipToSystemDto { SystemId = system2.Id };

        // Act
        var result = await _fleetService.AssignShipToSystemAsync(_testFractionId, ship.Id, dto);

        // Assert
        result.StationedInSystemId.ShouldBe(system2.Id);
        system1.StationedShipIds.ShouldNotContain(ship.Id);
        system2.StationedShipIds.ShouldContain(ship.Id);
    }

    #endregion

    #region GetFractionFleetAsync Tests

    [Fact]
    public async Task GetFractionFleetAsync_ShouldReturnCompleteFractionFleet()
    {
        // Arrange
        var state = CreateTestState();
        var system = new PlanetarySystem { Name = "Home System" };
        var ship1 = FractionShipTemplate.CreateDefault("Ship 1", ShipType.Corvette);
        var ship2 = FractionShipTemplate.CreateDefault("Ship 2", ShipType.Destroyer);
        ship1.StationedInSystemId = system.Id;
        state.PlanetarySystems.Add(system);
        state.Fleet.Add(ship1);
        state.Fleet.Add(ship2);
        SetupRepositoryWithState(state);

        // Act
        var result = await _fleetService.GetFractionFleetAsync(_testFractionId);

        // Assert
        result.FractionId.ShouldBe(_testFractionId);
        result.Fleet.Count.ShouldBe(2);
        result.PlanetarySystems.Count.ShouldBe(1);
        result.UnassignedShips.Count.ShouldBe(1);
        result.UnassignedShips[0].Name.ShouldBe("Ship 2");
    }

    [Fact]
    public async Task GetFractionFleetAsync_WithNoState_ShouldCreateNewState()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByFractionIdAsync(_testFractionId))
            .ReturnsAsync((FractionGameState?)null);
        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<FractionGameState>()))
            .ReturnsAsync((FractionGameState s) => s);

        // Act
        var result = await _fleetService.GetFractionFleetAsync(_testFractionId);

        // Assert
        result.FractionId.ShouldBe(_testFractionId);
        result.Fleet.ShouldBeEmpty();
        result.PlanetarySystems.ShouldBeEmpty();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<FractionGameState>()), Times.Once);
    }

    #endregion
}

