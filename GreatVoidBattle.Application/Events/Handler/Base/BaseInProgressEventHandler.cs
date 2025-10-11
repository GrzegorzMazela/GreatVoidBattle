using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Exceptions;

namespace GreatVoidBattle.Application.Events.Handler.Base;

public abstract class BaseInProgressEventHandler<TEvent> : BaseEventHandler<TEvent> where TEvent : BattleEvent
{
    public override Task HandleAsync(TEvent battleEvent, Core.Domains.BattleState battleState)
    {
        if (battleState.BattleStatus != Core.Domains.Enums.BattleStatus.InProgress)
        {
            throw new WrongBattleStatusException(battleState.BattleStatus);
        }
        return HandleInProgressEventAsync(battleEvent, battleState);
    }

    public abstract Task HandleInProgressEventAsync(TEvent battleEvent, Core.Domains.BattleState battleState);
}


