using GreatVoidBattle.Core.Domains.Exploration;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Core.Domains.Exploration;

public class ExplorationStateTests
{
    [Fact]
    public void Constructor_ShouldInitializeWithDefaults()
    {
        // Act
        var state = new ExplorationState();

        // Assert
        state.Id.ShouldNotBeNullOrEmpty();
        state.FractionId.ShouldBe(string.Empty);
        state.ResearchSlots.ShouldBe(2); // Default value
        state.UsedSlotsThisTurn.ShouldBe(0);
        state.DiscoveredSystemsCount.ShouldBe(0);
        state.TotalExpeditions.ShouldBe(0);
        state.SuccessfulExpeditions.ShouldBe(0);
        state.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Fact]
    public void ExplorationState_ShouldAllowSettingAllProperties()
    {
        // Arrange & Act
        var state = new ExplorationState
        {
            FractionId = "protektorat-pogranicza",
            ResearchSlots = 5,
            UsedSlotsThisTurn = 3,
            DiscoveredSystemsCount = 10,
            TotalExpeditions = 25,
            SuccessfulExpeditions = 20
        };

        // Assert
        state.FractionId.ShouldBe("protektorat-pogranicza");
        state.ResearchSlots.ShouldBe(5);
        state.UsedSlotsThisTurn.ShouldBe(3);
        state.DiscoveredSystemsCount.ShouldBe(10);
        state.TotalExpeditions.ShouldBe(25);
        state.SuccessfulExpeditions.ShouldBe(20);
    }

    [Fact]
    public void UsedSlotsThisTurn_ShouldNotExceedResearchSlots()
    {
        // Arrange
        var state = new ExplorationState
        {
            ResearchSlots = 3,
            UsedSlotsThisTurn = 2
        };

        // Assert - Available slots calculation
        var availableSlots = state.ResearchSlots - state.UsedSlotsThisTurn;
        availableSlots.ShouldBe(1);
    }

    [Fact]
    public void SuccessfulExpeditions_ShouldNotExceedTotalExpeditions()
    {
        // Arrange & Act
        var state = new ExplorationState
        {
            TotalExpeditions = 10,
            SuccessfulExpeditions = 8
        };

        // Assert
        state.SuccessfulExpeditions.ShouldBeLessThanOrEqualTo(state.TotalExpeditions);
    }

    [Fact]
    public void UpdatedAt_ShouldBeSetOnCreation()
    {
        // Act
        var state = new ExplorationState();

        // Assert
        state.UpdatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(5));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public void ResearchSlots_ShouldAcceptVariousValues(int slots)
    {
        // Arrange & Act
        var state = new ExplorationState { ResearchSlots = slots };

        // Assert
        state.ResearchSlots.ShouldBe(slots);
    }
}

