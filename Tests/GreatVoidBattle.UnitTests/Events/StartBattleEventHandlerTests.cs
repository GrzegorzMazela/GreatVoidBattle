using GreatVoidBattle.Application.Events;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Application.Managers;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using Microsoft.Extensions.Logging;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace GreatVoidBattle.UnitTests.Events;

public class StartBattleEventHandlerTests
{
    [Fact]
    public async Task StartBattleEventHandler_StartsBattle_Success()
    {
        // Arrange
        var battleState = BattleState.CreateNew("Test Battle");
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
        var battleState = BattleState.CreateNew("Test Battle");
        battleState.StartBattle(); // Set status to InProgress
        var battleManager = new BattleManager(battleState);

        var startBattleEvent = new StartBattleEvent
        {
            BattleId = battleManager.BattleId
        };



        // Assert
        //zmien to tak aby sprawdzalo czy zwraca dobry blad: 
        await Should.ThrowAsync<WrongBattleStatusException>(async () => await battleManager.ApplyEventAsync(startBattleEvent));
    }
}
