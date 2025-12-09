using GreatVoidBattle.Api.Controllers;
using GreatVoidBattle.Application.Dto.Exploration;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Core.Domains.Exploration;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Controllers;

public class ExplorationControllerTests
{
    private readonly ExplorationController _controller;
    private readonly Mock<IStarSystemRepository> _systemRepositoryMock;
    private readonly Mock<IExpeditionRepository> _expeditionRepositoryMock;
    private readonly Mock<ISystemDiscoveryRepository> _discoveryRepositoryMock;
    private readonly Mock<IExplorationStateRepository> _stateRepositoryMock;
    private readonly Mock<IGameSessionRepository> _sessionRepositoryMock;
    private readonly ExplorationService _service;

    private const string TestFractionId = "hegemonia-titanum";
    private const string TestSystemId = "system-1";

    public ExplorationControllerTests()
    {
        _systemRepositoryMock = new Mock<IStarSystemRepository>();
        _expeditionRepositoryMock = new Mock<IExpeditionRepository>();
        _discoveryRepositoryMock = new Mock<ISystemDiscoveryRepository>();
        _stateRepositoryMock = new Mock<IExplorationStateRepository>();
        _sessionRepositoryMock = new Mock<IGameSessionRepository>();
        var loggerMock = new Mock<ILogger<ExplorationService>>();

        _service = new ExplorationService(
            _systemRepositoryMock.Object,
            _expeditionRepositoryMock.Object,
            _discoveryRepositoryMock.Object,
            _stateRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            loggerMock.Object);

        _controller = new ExplorationController(_service);
    }

    private StarSystem CreateTestSystem(string? id = null, string name = "Test System")
    {
        return new StarSystem
        {
            Id = id ?? TestSystemId,
            Name = name,
            X = 100,
            Y = 200,
            Type = "resources"
        };
    }

    private ExplorationState CreateTestState()
    {
        return new ExplorationState
        {
            FractionId = TestFractionId,
            ResearchSlots = 3,
            UsedSlotsThisTurn = 0
        };
    }

    #region Star Systems Endpoints

    [Fact]
    public async Task GetAllSystems_ShouldReturnOkWithSystems()
    {
        // Arrange
        var systems = new List<StarSystem>
        {
            CreateTestSystem("sys-1", "Alpha"),
            CreateTestSystem("sys-2", "Beta")
        };
        _systemRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(systems);

        // Act
        var result = await _controller.GetAllSystems();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedSystems = okResult.Value.ShouldBeOfType<List<StarSystemAdminDto>>();
        returnedSystems.Count.ShouldBe(2);
    }

    [Fact]
    public async Task GetSystem_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);

        // Act
        var result = await _controller.GetSystem(TestSystemId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var dto = okResult.Value.ShouldBeOfType<StarSystemAdminDto>();
        dto.Name.ShouldBe("Test System");
    }

    [Fact]
    public async Task GetSystem_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        // Act
        var result = await _controller.GetSystem("invalid");

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreateSystem_ShouldReturnCreatedAtAction()
    {
        // Arrange
        var dto = new CreateStarSystemDto
        {
            Name = "New System",
            X = 100,
            Y = 200,
            Type = "habitable"
        };
        _systemRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<StarSystem>()))
            .ReturnsAsync((StarSystem s) => s);

        // Act
        var result = await _controller.CreateSystem(dto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        createdResult.ActionName.ShouldBe(nameof(_controller.GetSystem));
        var returnedDto = createdResult.Value.ShouldBeOfType<StarSystemAdminDto>();
        returnedDto.Name.ShouldBe("New System");
    }

    [Fact]
    public async Task UpdateSystem_WithValidId_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _systemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StarSystem>())).Returns(Task.CompletedTask);

        var dto = new UpdateStarSystemDto { Name = "Updated" };

        // Act
        var result = await _controller.UpdateSystem(TestSystemId, dto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedDto = okResult.Value.ShouldBeOfType<StarSystemAdminDto>();
        returnedDto.Name.ShouldBe("Updated");
    }

    [Fact]
    public async Task UpdateSystem_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        // Act
        var result = await _controller.UpdateSystem("invalid", new UpdateStarSystemDto());

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    #endregion

    #region Fraction Exploration Endpoints

    [Fact]
    public async Task GetKnownSystems_ShouldReturnOk()
    {
        // Arrange
        var discoveries = new List<SystemDiscovery>
        {
            new SystemDiscovery { FractionId = TestFractionId, SystemId = "sys-1", KnownName = "Alpha" }
        };
        _discoveryRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(discoveries);

        // Act
        var result = await _controller.GetKnownSystems(TestFractionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var returnedDiscoveries = okResult.Value.ShouldBeOfType<List<SystemDiscoveryDto>>();
        returnedDiscoveries.Count.ShouldBe(1);
    }

    [Fact]
    public async Task GetExplorationStatus_ShouldReturnOk()
    {
        // Arrange
        var state = CreateTestState();
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId))
            .ReturnsAsync(new List<Expedition>());
        _discoveryRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId))
            .ReturnsAsync(new List<SystemDiscovery>());

        // Act
        var result = await _controller.GetExplorationStatus(TestFractionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var status = okResult.Value.ShouldBeOfType<ExplorationStatusDto>();
        status.FractionId.ShouldBe(TestFractionId);
    }

    [Fact]
    public async Task SendExpedition_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        var system = CreateTestSystem();
        var state = CreateTestState();
        var session = new GameSession { CurrentTurn = 1 };

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _sessionRepositoryMock.Setup(r => r.GetActiveSessionAsync()).ReturnsAsync(session);
        _expeditionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Expedition>()))
            .ReturnsAsync((Expedition e) => e);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        var dto = new SendExpeditionDto { SystemId = TestSystemId };

        // Act
        var result = await _controller.SendExpedition(TestFractionId, dto);

        // Assert
        var createdResult = result.ShouldBeOfType<CreatedAtActionResult>();
        var expedition = createdResult.Value.ShouldBeOfType<ExpeditionDto>();
        expedition.SystemId.ShouldBe(TestSystemId);
    }

    [Fact]
    public async Task SendExpedition_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        var dto = new SendExpeditionDto { SystemId = "invalid" };

        // Act
        var result = await _controller.SendExpedition(TestFractionId, dto);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CancelExpedition_WithValidExpedition_ShouldReturnNoContent()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            Status = ExpeditionStatus.Pending
        };
        var state = CreateTestState();
        state.UsedSlotsThisTurn = 1;

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);
        _stateRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CancelExpedition(TestFractionId, "exp-1");

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task CancelExpedition_WithInvalidExpedition_ShouldReturnNotFound()
    {
        // Arrange
        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((Expedition?)null);

        // Act
        var result = await _controller.CancelExpedition(TestFractionId, "invalid");

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPendingExpeditions_ShouldReturnOk()
    {
        // Arrange
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId))
            .ReturnsAsync(new List<Expedition>());

        // Act
        var result = await _controller.GetPendingExpeditions(TestFractionId);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetExplorationHistory_ShouldReturnOk()
    {
        // Arrange
        _expeditionRepositoryMock.Setup(r => r.GetHistoryByFractionIdAsync(TestFractionId))
            .ReturnsAsync(new List<Expedition>());

        // Act
        var result = await _controller.GetExplorationHistory(TestFractionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var history = okResult.Value.ShouldBeOfType<ExplorationHistoryDto>();
        history.FractionId.ShouldBe(TestFractionId);
    }

    [Fact]
    public async Task GetResearchSlots_ShouldReturnOk()
    {
        // Arrange
        var state = CreateTestState();
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId))
            .ReturnsAsync(new List<Expedition>());

        // Act
        var result = await _controller.GetResearchSlots(TestFractionId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var slots = okResult.Value.ShouldBeOfType<FractionResearchSlotsDto>();
        slots.ResearchSlots.ShouldBe(3);
    }

    #endregion

    #region Admin Endpoints

    [Fact]
    public async Task SetResearchSlots_WithValidValue_ShouldReturnOk()
    {
        // Arrange
        var state = CreateTestState();
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        var dto = new SetResearchSlotsDto { Slots = 5 };

        // Act
        var result = await _controller.SetResearchSlots(TestFractionId, dto);

        // Assert
        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task SetResearchSlots_WithNegativeValue_ShouldReturnBadRequest()
    {
        // Arrange
        var dto = new SetResearchSlotsDto { Slots = -1 };

        // Act
        var result = await _controller.SetResearchSlots(TestFractionId, dto);

        // Assert
        result.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetAllPendingExpeditions_ShouldReturnOk()
    {
        // Arrange
        _expeditionRepositoryMock.Setup(r => r.GetAllPendingAsync())
            .ReturnsAsync(new List<Expedition>());

        // Act
        var result = await _controller.GetAllPendingExpeditions();

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task ResolveExpedition_WithValidExpedition_ShouldReturnOk()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            SystemId = TestSystemId,
            Status = ExpeditionStatus.Pending
        };
        var system = CreateTestSystem();

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);

        var dto = new ResolveExpeditionDto
        {
            Success = true,
            Message = "Found resources"
        };

        // Act
        var result = await _controller.ResolveExpedition("exp-1", dto);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var resolved = okResult.Value.ShouldBeOfType<ExpeditionAdminDto>();
        resolved.Status.ShouldBe("Approved");
    }

    [Fact]
    public async Task ResolveExpedition_WithInvalidExpedition_ShouldReturnNotFound()
    {
        // Arrange
        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((Expedition?)null);

        var dto = new ResolveExpeditionDto { Success = true };

        // Act
        var result = await _controller.ResolveExpedition("invalid", dto);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task RejectExpedition_WithValidExpedition_ShouldReturnNoContent()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            Status = ExpeditionStatus.Pending
        };

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);

        var request = new RejectExpeditionRequest { Reason = "Too far" };

        // Act
        var result = await _controller.RejectExpedition("exp-1", request);

        // Assert
        result.ShouldBeOfType<NoContentResult>();
    }

    [Fact]
    public async Task SendSystemInfo_WithValidSystem_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        var state = CreateTestState();

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _discoveryRepositoryMock.Setup(r => r.GetByFractionAndSystemAsync(TestFractionId, TestSystemId))
            .ReturnsAsync((SystemDiscovery?)null);
        _discoveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SystemDiscovery>()))
            .ReturnsAsync((SystemDiscovery d) => d);
        _stateRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        var dto = new SendSystemInfoDto
        {
            FractionId = TestFractionId,
            Message = "Intel"
        };

        // Act
        var result = await _controller.SendSystemInfo(TestSystemId, dto);

        // Assert
        result.ShouldBeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task SendSystemInfo_WithInvalidSystem_ShouldReturnNotFound()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        var dto = new SendSystemInfoDto { FractionId = TestFractionId };

        // Act
        var result = await _controller.SendSystemInfo("invalid", dto);

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetSystemNotes_WithValidSystem_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        system.AdminNotes = "Notes";
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);

        // Act
        var result = await _controller.GetSystemNotes(TestSystemId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var notes = okResult.Value.ShouldBeOfType<SystemNotesDto>();
        notes.AdminNotes.ShouldBe("Notes");
    }

    [Fact]
    public async Task GetSystemNotes_WithInvalidSystem_ShouldReturnNotFound()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        // Act
        var result = await _controller.GetSystemNotes("invalid");

        // Assert
        result.ShouldBeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task SaveSystemNotes_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _systemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StarSystem>())).Returns(Task.CompletedTask);

        var dto = new SystemNotesDto { AdminNotes = "New notes" };

        // Act
        var result = await _controller.SaveSystemNotes(TestSystemId, dto);

        // Assert
        result.ShouldBeOfType<OkResult>();
    }

    [Fact]
    public async Task GetSystemDiscoveryStatus_WithValidSystem_ShouldReturnOk()
    {
        // Arrange
        var system = CreateTestSystem();
        var session = new GameSession { FractionIds = new List<string> { TestFractionId } };

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _sessionRepositoryMock.Setup(r => r.GetActiveSessionAsync()).ReturnsAsync(session);
        _discoveryRepositoryMock.Setup(r => r.GetBySystemIdAsync(TestSystemId))
            .ReturnsAsync(new List<SystemDiscovery>());

        // Act
        var result = await _controller.GetSystemDiscoveryStatus(TestSystemId);

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        var status = okResult.Value.ShouldBeOfType<SystemDiscoveryStatusDto>();
        status.SystemId.ShouldBe(TestSystemId);
    }

    [Fact]
    public async Task GetAllResearchSlots_ShouldReturnOk()
    {
        // Arrange
        _stateRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<ExplorationState>());

        // Act
        var result = await _controller.GetAllResearchSlots();

        // Assert
        var okResult = result.ShouldBeOfType<OkObjectResult>();
        okResult.Value.ShouldBeOfType<AllResearchSlotsDto>();
    }

    [Fact]
    public async Task EndExplorationTurn_ShouldReturnOk()
    {
        // Arrange
        _stateRepositoryMock.Setup(r => r.ResetUsedSlotsForAllAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.EndExplorationTurn();

        // Assert
        result.ShouldBeOfType<OkResult>();
    }

    #endregion
}

