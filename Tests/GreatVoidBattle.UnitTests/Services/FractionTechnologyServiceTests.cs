using GreatVoidBattle.Application.Repositories;
using GreatVoidBattle.Application.Services;
using GreatVoidBattle.Core.Domains.GameState;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Services;

public class FractionTechnologyServiceTests
{
    private readonly Mock<IFractionGameStateRepository> _repositoryMock;
    private readonly Mock<ILogger<FractionTechnologyService>> _loggerMock;
    private readonly FractionTechnologyService _service;

    public FractionTechnologyServiceTests()
    {
        _repositoryMock = new Mock<IFractionGameStateRepository>();
        _loggerMock = new Mock<ILogger<FractionTechnologyService>>();
        _service = new FractionTechnologyService(_repositoryMock.Object, _loggerMock.Object);
    }

    private FractionGameState CreateTestState(string fractionId = "test-fraction")
    {
        return new FractionGameState
        {
            Id = Guid.NewGuid().ToString(),
            FractionId = fractionId,
            CurrentTier = 1,
            ResearchedTechnologiesInCurrentTier = 0
        };
    }

    #region HasTechnologyAsync Tests

    [Fact]
    public async Task HasTechnologyAsync_WhenFractionHasTechnology_ShouldReturnTrue()
    {
        // Arrange
        var state = CreateTestState();
        state.Technologies.Add(new FractionTechnology { TechnologyId = "laser-mk2" });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.HasTechnologyAsync("test-fraction", "laser-mk2");

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task HasTechnologyAsync_WhenFractionDoesNotHaveTechnology_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.HasTechnologyAsync("test-fraction", "unknown-tech");

        // Assert
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task HasTechnologyAsync_WhenFractionNotFound_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("unknown-fraction"))
            .ReturnsAsync((FractionGameState?)null);

        // Act
        var result = await _service.HasTechnologyAsync("unknown-fraction", "any-tech");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region AddTechnologyAsync Tests

    [Fact]
    public async Task AddTechnologyAsync_ShouldAddTechnologyToFraction()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        var technology = new FractionTechnology
        {
            TechnologyId = "new-tech",
            Source = TechnologySource.Research
        };

        // Act
        await _service.AddTechnologyAsync("test-fraction", technology);

        // Assert
        state.Technologies.Count.ShouldBe(1);
        state.Technologies[0].TechnologyId.ShouldBe("new-tech");
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task AddTechnologyAsync_WhenSourceIsResearch_ShouldIncrementCounter()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 0;
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        var technology = new FractionTechnology
        {
            TechnologyId = "researched-tech",
            Source = TechnologySource.Research
        };

        // Act
        await _service.AddTechnologyAsync("test-fraction", technology);

        // Assert
        state.ResearchedTechnologiesInCurrentTier.ShouldBe(1);
    }

    [Fact]
    public async Task AddTechnologyAsync_WhenSourceIsNotResearch_ShouldNotIncrementCounter()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 0;
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        var technology = new FractionTechnology
        {
            TechnologyId = "traded-tech",
            Source = TechnologySource.Trade
        };

        // Act
        await _service.AddTechnologyAsync("test-fraction", technology);

        // Assert
        state.ResearchedTechnologiesInCurrentTier.ShouldBe(0);
    }

    [Fact]
    public async Task AddTechnologyAsync_WhenFractionNotFound_ShouldNotThrow()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("unknown-fraction"))
            .ReturnsAsync((FractionGameState?)null);

        var technology = new FractionTechnology { TechnologyId = "tech" };

        // Act & Assert
        await Should.NotThrowAsync(() => _service.AddTechnologyAsync("unknown-fraction", technology));
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FractionGameState>()), Times.Never);
    }

    #endregion

    #region RemoveTechnologyAsync Tests

    [Fact]
    public async Task RemoveTechnologyAsync_ShouldRemoveTechnologyFromFraction()
    {
        // Arrange
        var state = CreateTestState();
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "to-remove",
            Source = TechnologySource.Trade 
        });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        await _service.RemoveTechnologyAsync("test-fraction", "to-remove");

        // Assert
        state.Technologies.Count.ShouldBe(0);
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task RemoveTechnologyAsync_WhenTechWasResearched_ShouldDecrementCounter()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchedTechnologiesInCurrentTier = 2;
        state.Technologies.Add(new FractionTechnology 
        { 
            TechnologyId = "researched-tech",
            Source = TechnologySource.Research 
        });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        await _service.RemoveTechnologyAsync("test-fraction", "researched-tech");

        // Assert
        state.ResearchedTechnologiesInCurrentTier.ShouldBe(1);
    }

    [Fact]
    public async Task RemoveTechnologyAsync_WhenTechNotFound_ShouldNotUpdate()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        await _service.RemoveTechnologyAsync("test-fraction", "non-existent-tech");

        // Assert
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FractionGameState>()), Times.Never);
    }

    #endregion

    #region AdvanceTierAsync Tests

    [Fact]
    public async Task AdvanceTierAsync_WhenRequirementsMet_ShouldAdvanceTier()
    {
        // Arrange
        var state = CreateTestState();
        state.CurrentTier = 1;
        state.ResearchedTechnologiesInCurrentTier = 15; // Meets requirement (15 needed)
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.AdvanceTierAsync("test-fraction");

        // Assert
        result.ShouldBeTrue();
        state.CurrentTier.ShouldBe(2);
        state.ResearchedTechnologiesInCurrentTier.ShouldBe(0);
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task AdvanceTierAsync_WhenRequirementsNotMet_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        state.CurrentTier = 1;
        state.ResearchedTechnologiesInCurrentTier = 1; // Not enough
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.AdvanceTierAsync("test-fraction");

        // Assert
        result.ShouldBeFalse();
        state.CurrentTier.ShouldBe(1); // Unchanged
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<FractionGameState>()), Times.Never);
    }

    [Fact]
    public async Task AdvanceTierAsync_WhenFractionNotFound_ShouldReturnFalse()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("unknown-fraction"))
            .ReturnsAsync((FractionGameState?)null);

        // Act
        var result = await _service.AdvanceTierAsync("unknown-fraction");

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region AddResearchRequestAsync Tests

    [Fact]
    public async Task AddResearchRequestAsync_ShouldAddRequest()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        var request = new ResearchRequest
        {
            TechnologyId = "new-research",
            Status = ResearchRequestStatus.Pending
        };

        // Act
        var result = await _service.AddResearchRequestAsync("test-fraction", request);

        // Assert
        result.ShouldBeTrue();
        state.ResearchRequests.Count.ShouldBe(1);
        _repositoryMock.Verify(r => r.UpdateAsync(state), Times.Once);
    }

    [Fact]
    public async Task AddResearchRequestAsync_WhenPendingRequestExists_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest
        {
            TechnologyId = "existing-request",
            Status = ResearchRequestStatus.Pending
        });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        var request = new ResearchRequest
        {
            TechnologyId = "existing-request",
            Status = ResearchRequestStatus.Pending
        };

        // Act
        var result = await _service.AddResearchRequestAsync("test-fraction", request);

        // Assert
        result.ShouldBeFalse();
        state.ResearchRequests.Count.ShouldBe(1); // No new request added
    }

    #endregion

    #region ApproveResearchRequestAsync Tests

    [Fact]
    public async Task ApproveResearchRequestAsync_ShouldApprovePendingRequest()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest
        {
            TechnologyId = "pending-tech",
            Status = ResearchRequestStatus.Pending
        });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.ApproveResearchRequestAsync("test-fraction", "pending-tech", "Approved!");

        // Assert
        result.ShouldBeTrue();
        state.ResearchRequests[0].Status.ShouldBe(ResearchRequestStatus.Approved);
        state.ResearchRequests[0].AdminComment.ShouldBe("Approved!");
        state.Technologies.Count.ShouldBe(1);
        state.Technologies[0].TechnologyId.ShouldBe("pending-tech");
        state.ResearchedTechnologiesInCurrentTier.ShouldBe(1);
    }

    [Fact]
    public async Task ApproveResearchRequestAsync_WhenNoRequestFound_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.ApproveResearchRequestAsync("test-fraction", "non-existent", null);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region RejectResearchRequestAsync Tests

    [Fact]
    public async Task RejectResearchRequestAsync_ShouldRejectPendingRequest()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest
        {
            TechnologyId = "pending-tech",
            Status = ResearchRequestStatus.Pending
        });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.RejectResearchRequestAsync("test-fraction", "pending-tech", "Not approved");

        // Assert
        result.ShouldBeTrue();
        state.ResearchRequests[0].Status.ShouldBe(ResearchRequestStatus.Rejected);
        state.ResearchRequests[0].AdminComment.ShouldBe("Not approved");
        state.Technologies.Count.ShouldBe(0); // No technology added
    }

    [Fact]
    public async Task RejectResearchRequestAsync_WhenNoRequestFound_ShouldReturnFalse()
    {
        // Arrange
        var state = CreateTestState();
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.RejectResearchRequestAsync("test-fraction", "non-existent", null);

        // Assert
        result.ShouldBeFalse();
    }

    #endregion

    #region GetPendingResearchRequestsAsync Tests

    [Fact]
    public async Task GetPendingResearchRequestsAsync_ShouldReturnOnlyPendingRequests()
    {
        // Arrange
        var state = CreateTestState();
        state.ResearchRequests.Add(new ResearchRequest { TechnologyId = "pending-1", Status = ResearchRequestStatus.Pending });
        state.ResearchRequests.Add(new ResearchRequest { TechnologyId = "approved", Status = ResearchRequestStatus.Approved });
        state.ResearchRequests.Add(new ResearchRequest { TechnologyId = "pending-2", Status = ResearchRequestStatus.Pending });
        state.ResearchRequests.Add(new ResearchRequest { TechnologyId = "rejected", Status = ResearchRequestStatus.Rejected });
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("test-fraction"))
            .ReturnsAsync(state);

        // Act
        var result = await _service.GetPendingResearchRequestsAsync("test-fraction");

        // Assert
        result.Count.ShouldBe(2);
        result.All(r => r.Status == ResearchRequestStatus.Pending).ShouldBeTrue();
    }

    [Fact]
    public async Task GetPendingResearchRequestsAsync_WhenFractionNotFound_ShouldReturnEmptyList()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByFractionIdAsync("unknown-fraction"))
            .ReturnsAsync((FractionGameState?)null);

        // Act
        var result = await _service.GetPendingResearchRequestsAsync("unknown-fraction");

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);
    }

    #endregion
}

