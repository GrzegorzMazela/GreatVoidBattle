using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.InProgress.Handler;

internal class EndOfTurnEventHandler : BaseInProgressEventHandler<EndOfTurnEvent>
{
    public override Task HandleInProgressEventAsync(EndOfTurnEvent battleEvent, BattleState battleState)
    {
        battleState.EndOfTurn();
        return Task.CompletedTask;
    }
}