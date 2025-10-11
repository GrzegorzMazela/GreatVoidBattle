using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Application.Exceptions;

namespace GreatVoidBattle.Application.Events.Handler.Base;

public abstract class BasePreparationEventHandler<TEvent> : BaseEventHandler<TEvent> where TEvent : BattleEvent
{
    public override Task HandleAsync(TEvent battleEvent, Core.Domains.BattleState battleState)
    {
        if (battleState.BattleStatus != Core.Domains.Enums.BattleStatus.Preparation)
        {
            throw new WrongBattleStatusException(battleState.BattleStatus);
        }
        return HandlePreparationEventAsync(battleEvent, battleState);
    }

    public abstract Task HandlePreparationEventAsync(TEvent battleEvent, Core.Domains.BattleState battleState);
}