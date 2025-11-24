using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Shouldly;

namespace GreatVoidBattle.UnitTests.Events;

public class StartBattleEventHandlerTests
{
    [Fact]
    public async Task StartBattleEventHandler_StartsBattle_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        var battleManager = new BattleManager(battleState);

        // Ensure initial status is Preparation
        battleManager.BattleState.BattleStatus.ShouldBe(BattleStatus.Preparation);

        var startBattleEvent = new StartBattleEvent
        {
            BattleId = battleManager.BattleId
        };

        // Act
        await battleManager.ApplyEventAsync(startBattleEvent);

        // Assert
        battleManager.BattleState.BattleStatus.ShouldBe(BattleStatus.InProgress);
    }

    [Fact]
    public async Task StartBattleEventHandler_DoesNotChangeStatusIfAlreadyStarted()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle", 500, 500);
        battleState.StartBattle(); // Set status to InProgress
        var battleManager = new BattleManager(battleState);

        var startBattleEvent = new StartBattleEvent
        {
            BattleId = battleManager.BattleId
        };

        // Assert
        var exception = await Should.ThrowAsync<Exception>(async () => await battleManager.ApplyEventAsync(startBattleEvent));
        (exception.InnerException ?? exception).ShouldBeOfType<WrongBattleStatusException>();
    }
}