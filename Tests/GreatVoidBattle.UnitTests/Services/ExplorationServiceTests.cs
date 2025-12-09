using GreatVoidBattle.Application.Dto.Exploration;
using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Core.Domains.Exploration;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Services;

public class ExplorationServiceTests
{
    private readonly Mock<IStarSystemRepository> _systemRepositoryMock;
    private readonly Mock<IExpeditionRepository> _expeditionRepositoryMock;
    private readonly Mock<ISystemDiscoveryRepository> _discoveryRepositoryMock;
    private readonly Mock<IExplorationStateRepository> _stateRepositoryMock;
    private readonly Mock<IGameSessionRepository> _sessionRepositoryMock;
    private readonly Mock<ILogger<ExplorationService>> _loggerMock;
    private readonly ExplorationService _service;

    private const string TestFractionId = "hegemonia-titanum";
    private const string TestSystemId = "system-1";

    public ExplorationServiceTests()
    {
        _systemRepositoryMock = new Mock<IStarSystemRepository>();
        _expeditionRepositoryMock = new Mock<IExpeditionRepository>();
        _discoveryRepositoryMock = new Mock<ISystemDiscoveryRepository>();
        _stateRepositoryMock = new Mock<IExplorationStateRepository>();
        _sessionRepositoryMock = new Mock<IGameSessionRepository>();
        _loggerMock = new Mock<ILogger<ExplorationService>>();

        _service = new ExplorationService(
            _systemRepositoryMock.Object,
            _expeditionRepositoryMock.Object,
            _discoveryRepositoryMock.Object,
            _stateRepositoryMock.Object,
            _sessionRepositoryMock.Object,
            _loggerMock.Object);
    }

    private StarSystem CreateTestSystem(string? id = null, string name = "Test System")
    {
        return new StarSystem
        {
            Id = id ?? TestSystemId,
            Name = name,
            X = 100,
            Y = 200,
            Type = "resources",
            Description = "A test system",
            Resources = "Minerals"
        };
    }

    private ExplorationState CreateTestState(string? fractionId = null)
    {
        return new ExplorationState
        {
            Id = "state-1",
            FractionId = fractionId ?? TestFractionId,
            ResearchSlots = 3,
            UsedSlotsThisTurn = 0
        };
    }

    private GameSession CreateTestSession()
    {
        return new GameSession
        {
            Id = "session-1",
            CurrentTurn = 5,
            FractionIds = new List<string> { "hegemonia-titanum", "shimura-incorporated", "protektorat-pogranicza" }
        };
    }

    #region Star Systems Tests

    [Fact]
    public async Task GetAllSystemsAsync_ShouldReturnAllSystems()
    {
        // Arrange
        var systems = new List<StarSystem>
        {
            CreateTestSystem("sys-1", "Alpha"),
            CreateTestSystem("sys-2", "Beta"),
            CreateTestSystem("sys-3", "Gamma")
        };
        _systemRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(systems);

        // Act
        var result = await _service.GetAllSystemsAsync();

        // Assert
        result.Count.ShouldBe(3);
        result[0].Name.ShouldBe("Alpha");
        result[1].Name.ShouldBe("Beta");
        result[2].Name.ShouldBe("Gamma");
    }

    [Fact]
    public async Task GetSystemAsync_WithValidId_ShouldReturnSystem()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);

        // Act
        var result = await _service.GetSystemAsync(TestSystemId);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Test System");
        result.X.ShouldBe(100);
        result.Y.ShouldBe(200);
    }

    [Fact]
    public async Task GetSystemAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        // Act
        var result = await _service.GetSystemAsync("invalid");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CreateSystemAsync_ShouldCreateNewSystem()
    {
        // Arrange
        var dto = new CreateStarSystemDto
        {
            Name = "New System",
            X = 150,
            Y = 250,
            Type = "habitable",
            Description = "A new habitable system",
            Resources = "Water"
        };
        _systemRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<StarSystem>()))
            .ReturnsAsync((StarSystem s) => s);

        // Act
        var result = await _service.CreateSystemAsync(dto);

        // Assert
        result.Name.ShouldBe("New System");
        result.X.ShouldBe(150);
        result.Y.ShouldBe(250);
        result.Type.ShouldBe("habitable");
        _systemRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<StarSystem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSystemAsync_WithValidId_ShouldUpdateSystem()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _systemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StarSystem>())).Returns(Task.CompletedTask);

        var dto = new UpdateStarSystemDto
        {
            Name = "Updated Name",
            Type = "strategic"
        };

        // Act
        var result = await _service.UpdateSystemAsync(TestSystemId, dto);

        // Assert
        result.ShouldNotBeNull();
        result.Name.ShouldBe("Updated Name");
        result.Type.ShouldBe("strategic");
        _systemRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<StarSystem>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSystemAsync_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        // Act
        var result = await _service.UpdateSystemAsync("invalid", new UpdateStarSystemDto());

        // Assert
        result.ShouldBeNull();
    }

    #endregion

    #region Expedition Tests

    [Fact]
    public async Task SendExpeditionAsync_WithValidData_ShouldCreateExpedition()
    {
        // Arrange
        var system = CreateTestSystem();
        var state = CreateTestState();
        var session = CreateTestSession();

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _sessionRepositoryMock.Setup(r => r.GetActiveSessionAsync()).ReturnsAsync(session);
        _expeditionRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Expedition>()))
            .ReturnsAsync((Expedition e) => e);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        var dto = new SendExpeditionDto
        {
            SystemId = TestSystemId,
            Comment = "Exploring for resources"
        };

        // Act
        var result = await _service.SendExpeditionAsync(TestFractionId, dto);

        // Assert
        result.ShouldNotBeNull();
        result.SystemId.ShouldBe(TestSystemId);
        result.SystemName.ShouldBe("Test System");
        result.FractionId.ShouldBe(TestFractionId);
        result.Status.ShouldBe("Pending");
        result.SentAtTurn.ShouldBe(5);
        _expeditionRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Expedition>()), Times.Once);
    }

    [Fact]
    public async Task SendExpeditionAsync_WithInvalidSystem_ShouldReturnNull()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        var dto = new SendExpeditionDto { SystemId = "invalid" };

        // Act
        var result = await _service.SendExpeditionAsync(TestFractionId, dto);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task SendExpeditionAsync_WithNoAvailableSlots_ShouldReturnNull()
    {
        // Arrange
        var system = CreateTestSystem();
        var state = CreateTestState();
        state.UsedSlotsThisTurn = state.ResearchSlots; // All slots used

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);

        var dto = new SendExpeditionDto { SystemId = TestSystemId };

        // Act
        var result = await _service.SendExpeditionAsync(TestFractionId, dto);

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task CancelExpeditionAsync_WithValidPendingExpedition_ShouldCancel()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            SystemId = TestSystemId,
            Status = ExpeditionStatus.Pending
        };
        var state = CreateTestState();
        state.UsedSlotsThisTurn = 1;

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);
        _stateRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.CancelExpeditionAsync(TestFractionId, "exp-1");

        // Assert
        result.ShouldBeTrue();
        expedition.Status.ShouldBe(ExpeditionStatus.Cancelled);
    }

    [Fact]
    public async Task CancelExpeditionAsync_WithWrongFraction_ShouldReturnFalse()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = "other-fraction",
            Status = ExpeditionStatus.Pending
        };

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);

        // Act
        var result = await _service.CancelExpeditionAsync(TestFractionId, "exp-1");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task CancelExpeditionAsync_WithNonPendingExpedition_ShouldReturnFalse()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            Status = ExpeditionStatus.Approved
        };

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);

        // Act
        var result = await _service.CancelExpeditionAsync(TestFractionId, "exp-1");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task GetPendingExpeditionsAsync_ShouldReturnPendingExpeditions()
    {
        // Arrange
        var expeditions = new List<Expedition>
        {
            new Expedition { Id = "exp-1", FractionId = TestFractionId, SystemId = "sys-1", Status = ExpeditionStatus.Pending },
            new Expedition { Id = "exp-2", FractionId = TestFractionId, SystemId = "sys-2", Status = ExpeditionStatus.Pending }
        };
        var system1 = CreateTestSystem("sys-1", "System 1");
        var system2 = CreateTestSystem("sys-2", "System 2");

        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId)).ReturnsAsync(expeditions);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-1")).ReturnsAsync(system1);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-2")).ReturnsAsync(system2);

        // Act
        var result = await _service.GetPendingExpeditionsAsync(TestFractionId);

        // Assert
        result.Count.ShouldBe(2);
        result[0].SystemName.ShouldBe("System 1");
        result[1].SystemName.ShouldBe("System 2");
    }

    #endregion

    #region Admin Operations Tests

    [Fact]
    public async Task GetAllPendingExpeditionsAsync_ShouldReturnAllPending()
    {
        // Arrange
        var expeditions = new List<Expedition>
        {
            new Expedition { Id = "exp-1", FractionId = "faction-1", SystemId = "sys-1", Status = ExpeditionStatus.Pending },
            new Expedition { Id = "exp-2", FractionId = "faction-2", SystemId = "sys-2", Status = ExpeditionStatus.Pending }
        };
        var system1 = CreateTestSystem("sys-1", "Alpha");
        var system2 = CreateTestSystem("sys-2", "Beta");

        _expeditionRepositoryMock.Setup(r => r.GetAllPendingAsync()).ReturnsAsync(expeditions);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-1")).ReturnsAsync(system1);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-2")).ReturnsAsync(system2);

        // Act
        var result = await _service.GetAllPendingExpeditionsAsync();

        // Assert
        result.Count.ShouldBe(2);
    }

    [Fact]
    public async Task ResolveExpeditionAsync_WithSuccess_ShouldApproveAndCreateDiscovery()
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
        var state = CreateTestState();

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _stateRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);
        _discoveryRepositoryMock.Setup(r => r.GetByFractionAndSystemAsync(TestFractionId, TestSystemId))
            .ReturnsAsync((SystemDiscovery?)null);
        _discoveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SystemDiscovery>()))
            .ReturnsAsync((SystemDiscovery d) => d);

        var dto = new ResolveExpeditionDto
        {
            Success = true,
            Message = "Found resources!",
            AdminComment = "Approved",
            DiscoveredInfo = new DiscoveredSystemInfoDto
            {
                SystemName = "Test System",
                Type = "resources",
                Description = "Rich in minerals",
                Resources = "Tritium"
            }
        };

        // Act
        var result = await _service.ResolveExpeditionAsync("exp-1", dto);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Approved");
        expedition.Status.ShouldBe(ExpeditionStatus.Approved);
        expedition.ResolvedAt.ShouldNotBeNull();
        _discoveryRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<SystemDiscovery>()), Times.Once);
    }

    [Fact]
    public async Task ResolveExpeditionAsync_WithFailure_ShouldReject()
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
            Success = false,
            Message = "Expedition failed",
            AdminComment = "Bad weather"
        };

        // Act
        var result = await _service.ResolveExpeditionAsync("exp-1", dto);

        // Assert
        result.ShouldNotBeNull();
        result.Status.ShouldBe("Rejected");
        expedition.Status.ShouldBe(ExpeditionStatus.Rejected);
    }

    [Fact]
    public async Task RejectExpeditionAsync_ShouldRejectExpedition()
    {
        // Arrange
        var expedition = new Expedition
        {
            Id = "exp-1",
            FractionId = TestFractionId,
            SystemId = TestSystemId,
            Status = ExpeditionStatus.Pending
        };

        _expeditionRepositoryMock.Setup(r => r.GetByIdAsync("exp-1")).ReturnsAsync(expedition);
        _expeditionRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Expedition>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.RejectExpeditionAsync("exp-1", "Invalid target");

        // Assert
        result.ShouldBeTrue();
        expedition.Status.ShouldBe(ExpeditionStatus.Rejected);
        expedition.AdminComment.ShouldBe("Invalid target");
    }

    [Fact]
    public async Task SetResearchSlotsAsync_WithValidValue_ShouldUpdateSlots()
    {
        // Arrange
        var state = CreateTestState();
        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.SetResearchSlotsAsync(TestFractionId, 5);

        // Assert
        result.ShouldBeTrue();
        state.ResearchSlots.ShouldBe(5);
    }

    [Fact]
    public async Task SetResearchSlotsAsync_WithNegativeValue_ShouldReturnFalse()
    {
        // Act
        var result = await _service.SetResearchSlotsAsync(TestFractionId, -1);

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task EndExplorationTurnAsync_ShouldResetAllSlots()
    {
        // Arrange
        _stateRepositoryMock.Setup(r => r.ResetUsedSlotsForAllAsync()).Returns(Task.CompletedTask);

        // Act
        await _service.EndExplorationTurnAsync();

        // Assert
        _stateRepositoryMock.Verify(r => r.ResetUsedSlotsForAllAsync(), Times.Once);
    }

    #endregion

    #region Exploration Status Tests

    [Fact]
    public async Task GetExplorationStatusAsync_ShouldReturnStatus()
    {
        // Arrange
        var state = CreateTestState();
        state.DiscoveredSystemsCount = 5;
        state.TotalExpeditions = 10;
        state.SuccessfulExpeditions = 8;
        state.UsedSlotsThisTurn = 1;

        var pendingExpeditions = new List<Expedition>
        {
            new Expedition { Status = ExpeditionStatus.Pending }
        };
        var discoveries = new List<SystemDiscovery>
        {
            new SystemDiscovery(),
            new SystemDiscovery()
        };

        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId)).ReturnsAsync(pendingExpeditions);
        _discoveryRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(discoveries);

        // Act
        var result = await _service.GetExplorationStatusAsync(TestFractionId);

        // Assert
        result.FractionId.ShouldBe(TestFractionId);
        result.ResearchSlots.ShouldBe(3);
        result.UsedSlotsThisTurn.ShouldBe(1);
        result.AvailableSlots.ShouldBe(2);
        result.DiscoveredSystemsCount.ShouldBe(2); // From discoveries count
        result.TotalExpeditions.ShouldBe(10);
        result.SuccessfulExpeditions.ShouldBe(8);
        result.PendingExpeditionsCount.ShouldBe(1);
    }

    [Fact]
    public async Task GetKnownSystemsAsync_ShouldReturnDiscoveries()
    {
        // Arrange
        var discoveries = new List<SystemDiscovery>
        {
            new SystemDiscovery { SystemId = "sys-1", KnownName = "Alpha", KnowledgeLevel = 2 },
            new SystemDiscovery { SystemId = "sys-2", KnownName = "Beta", KnowledgeLevel = 3 }
        };

        _discoveryRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId)).ReturnsAsync(discoveries);

        // Act
        var result = await _service.GetKnownSystemsAsync(TestFractionId);

        // Assert
        result.Count.ShouldBe(2);
        result[0].KnownName.ShouldBe("Alpha");
        result[1].KnownName.ShouldBe("Beta");
    }

    [Fact]
    public async Task GetResearchSlotsAsync_ShouldReturnSlotInfo()
    {
        // Arrange
        var state = CreateTestState();
        var pendingExpeditions = new List<Expedition>
        {
            new Expedition { Status = ExpeditionStatus.Pending },
            new Expedition { Status = ExpeditionStatus.Pending }
        };

        _stateRepositoryMock.Setup(r => r.GetOrCreateByFractionIdAsync(TestFractionId)).ReturnsAsync(state);
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(TestFractionId)).ReturnsAsync(pendingExpeditions);

        // Act
        var result = await _service.GetResearchSlotsAsync(TestFractionId);

        // Assert
        result.FractionId.ShouldBe(TestFractionId);
        result.ResearchSlots.ShouldBe(3);
        result.UsedSlotsThisTurn.ShouldBe(0);
        result.PendingExpeditions.ShouldBe(2);
    }

    #endregion

    #region System Notes Tests

    [Fact]
    public async Task GetSystemNotesAsync_ShouldReturnNotes()
    {
        // Arrange
        var system = CreateTestSystem();
        system.AdminNotes = "Important notes";
        system.SecretInfo = "Secret data";

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);

        // Act
        var result = await _service.GetSystemNotesAsync(TestSystemId);

        // Assert
        result.ShouldNotBeNull();
        result.SystemId.ShouldBe(TestSystemId);
        result.AdminNotes.ShouldBe("Important notes");
        result.SecretInfo.ShouldBe("Secret data");
    }

    [Fact]
    public async Task SaveSystemNotesAsync_ShouldSaveNotes()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _systemRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<StarSystem>())).Returns(Task.CompletedTask);

        var dto = new SystemNotesDto
        {
            SystemId = TestSystemId,
            AdminNotes = "Updated notes",
            SecretInfo = "Updated secret"
        };

        // Act
        var result = await _service.SaveSystemNotesAsync(TestSystemId, dto);

        // Assert
        result.ShouldBeTrue();
        system.AdminNotes.ShouldBe("Updated notes");
        system.SecretInfo.ShouldBe("Updated secret");
    }

    #endregion

    #region Discovery Status Tests

    [Fact]
    public async Task GetSystemDiscoveryStatusAsync_ShouldReturnStatusForAllFractions()
    {
        // Arrange
        var system = CreateTestSystem();
        var session = CreateTestSession();
        var discoveries = new List<SystemDiscovery>
        {
            new SystemDiscovery { FractionId = "hegemonia-titanum", KnowledgeLevel = 3, FullyExplored = true }
        };

        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _sessionRepositoryMock.Setup(r => r.GetActiveSessionAsync()).ReturnsAsync(session);
        _discoveryRepositoryMock.Setup(r => r.GetBySystemIdAsync(TestSystemId)).ReturnsAsync(discoveries);

        // Act
        var result = await _service.GetSystemDiscoveryStatusAsync(TestSystemId);

        // Assert
        result.ShouldNotBeNull();
        result.SystemId.ShouldBe(TestSystemId);
        result.Fractions.Count.ShouldBe(3);
        
        var hegemoniaStatus = result.Fractions.First(f => f.FractionId == "hegemonia-titanum");
        hegemoniaStatus.IsDiscovered.ShouldBeTrue();
        hegemoniaStatus.KnowledgeLevel.ShouldBe(3);
        hegemoniaStatus.FullyExplored.ShouldBeTrue();

        var shimuraStatus = result.Fractions.First(f => f.FractionId == "shimura-incorporated");
        shimuraStatus.IsDiscovered.ShouldBeFalse();
        shimuraStatus.KnowledgeLevel.ShouldBe(0);
    }

    [Fact]
    public async Task GetAllResearchSlotsAsync_ShouldReturnAllFractionSlots()
    {
        // Arrange
        var states = new List<ExplorationState>
        {
            new ExplorationState { FractionId = "faction-1", ResearchSlots = 3, UsedSlotsThisTurn = 1 },
            new ExplorationState { FractionId = "faction-2", ResearchSlots = 5, UsedSlotsThisTurn = 2 }
        };

        _stateRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(states);
        _expeditionRepositoryMock.Setup(r => r.GetPendingByFractionIdAsync(It.IsAny<string>()))
            .ReturnsAsync(new List<Expedition>());

        // Act
        var result = await _service.GetAllResearchSlotsAsync();

        // Assert
        result.Fractions.Count.ShouldBe(2);
        result.Fractions[0].ResearchSlots.ShouldBe(3);
        result.Fractions[1].ResearchSlots.ShouldBe(5);
    }

    #endregion

    #region Send System Info Tests

    [Fact]
    public async Task SendSystemInfoAsync_ShouldCreateDiscoveryForFraction()
    {
        // Arrange
        var system = CreateTestSystem();
        _systemRepositoryMock.Setup(r => r.GetByIdAsync(TestSystemId)).ReturnsAsync(system);
        _discoveryRepositoryMock.Setup(r => r.GetByFractionAndSystemAsync(TestFractionId, TestSystemId))
            .ReturnsAsync((SystemDiscovery?)null);
        _discoveryRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<SystemDiscovery>()))
            .ReturnsAsync((SystemDiscovery d) => d);
        _stateRepositoryMock.Setup(r => r.GetByFractionIdAsync(TestFractionId))
            .ReturnsAsync(CreateTestState());
        _stateRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<ExplorationState>()))
            .Returns(Task.CompletedTask);

        var dto = new SendSystemInfoDto
        {
            FractionId = TestFractionId,
            Message = "Intel received",
            KnowledgeLevel = 2,
            Info = new DiscoveredSystemInfoDto
            {
                SystemName = "Test System",
                Type = "resources"
            }
        };

        // Act
        var result = await _service.SendSystemInfoAsync(TestSystemId, dto);

        // Assert
        result.ShouldBeTrue();
        _discoveryRepositoryMock.Verify(r => r.CreateAsync(It.Is<SystemDiscovery>(
            d => d.Source == DiscoverySource.Admin && d.KnowledgeLevel == 2)), Times.Once);
    }

    [Fact]
    public async Task SendSystemInfoAsync_WithInvalidSystem_ShouldReturnFalse()
    {
        // Arrange
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("invalid")).ReturnsAsync((StarSystem?)null);

        var dto = new SendSystemInfoDto { FractionId = TestFractionId };

        // Act
        var result = await _service.SendSystemInfoAsync("invalid", dto);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region Exploration History Tests

    [Fact]
    public async Task GetExplorationHistoryAsync_ShouldReturnHistory()
    {
        // Arrange
        var history = new List<Expedition>
        {
            new Expedition 
            { 
                Id = "exp-1", 
                FractionId = TestFractionId, 
                SystemId = "sys-1", 
                Status = ExpeditionStatus.Approved,
                SentAtTurn = 3,
                Result = new ExpeditionResult { Message = "Success!" }
            },
            new Expedition 
            { 
                Id = "exp-2", 
                FractionId = TestFractionId, 
                SystemId = "sys-2", 
                Status = ExpeditionStatus.Rejected,
                SentAtTurn = 2,
                Result = new ExpeditionResult { Message = "Failed" }
            }
        };

        _expeditionRepositoryMock.Setup(r => r.GetHistoryByFractionIdAsync(TestFractionId)).ReturnsAsync(history);
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-1")).ReturnsAsync(CreateTestSystem("sys-1", "Alpha"));
        _systemRepositoryMock.Setup(r => r.GetByIdAsync("sys-2")).ReturnsAsync(CreateTestSystem("sys-2", "Beta"));

        // Act
        var result = await _service.GetExplorationHistoryAsync(TestFractionId);

        // Assert
        result.FractionId.ShouldBe(TestFractionId);
        result.Entries.Count.ShouldBe(2);
        result.Entries[0].Type.ShouldBe("expedition");
    }

    #endregion
}

