using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class UpdateFractionEventHandlerTests
{
    [Fact]
    public async Task UpdateFractionEventHandler_UpdateFractionName_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var fraction = FractionState.CreateNew("Original Name", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        var battleManager = new BattleManager(battleState);

        var updateEvent = new UpdateFractionEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            Name = "Updated Name",
            PlayerName = "Player 1",
            FractionColor = "#FF0000"
        };

        // Act
        await battleManager.ApplyEventAsync(updateEvent);

        // Assert
        var updatedFraction = battleManager.BattleState.Fractions.First(f => f.FractionId == fraction.FractionId);
        updatedFraction.FractionName.ShouldBe("Updated Name");
        updatedFraction.PlayerName.ShouldBe("Player 1");
        updatedFraction.FractionColor.ShouldBe("#FF0000");
    }

    [Fact]
    public async Task UpdateFractionEventHandler_UpdateAllProperties_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var fraction = FractionState.CreateNew("Original Name", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        var battleManager = new BattleManager(battleState);

        var updateEvent = new UpdateFractionEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            Name = "New Name",
            PlayerName = "New Player",
            FractionColor = "#00FF00"
        };

        // Act
        await battleManager.ApplyEventAsync(updateEvent);

        // Assert
        var updatedFraction = battleManager.BattleState.Fractions.First(f => f.FractionId == fraction.FractionId);
        updatedFraction.FractionName.ShouldBe("New Name");
        updatedFraction.PlayerName.ShouldBe("New Player");
        updatedFraction.FractionColor.ShouldBe("#00FF00");
    }

    [Fact]
    public async Task UpdateFractionEventHandler_WithInvalidFractionId_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var battleManager = new BattleManager(battleState);

        var updateEvent = new UpdateFractionEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = Guid.NewGuid(),
            Name = "New Name",
            PlayerName = "New Player",
            FractionColor = "#00FF00"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(updateEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<InvalidOperationException>();
    }

    [Fact]
    public async Task UpdateFractionEventHandler_WhenBattleInProgress_ThrowsException()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var fraction = FractionState.CreateNew("Original Name", "Player 1", "#FF0000");
        battleState.AddFraction(fraction);
        battleState.StartBattle();
        var battleManager = new BattleManager(battleState);

        var updateEvent = new UpdateFractionEvent
        {
            BattleId = battleManager.BattleId,
            FractionId = fraction.FractionId,
            Name = "New Name",
            PlayerName = "New Player",
            FractionColor = "#00FF00"
        };

        // Act & Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(updateEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}
