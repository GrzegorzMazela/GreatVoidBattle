using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler.Base;

public abstract class BaseEventHandler<TEvent> where TEvent : BattleEvent
{
    public abstract Task HandleAsync(TEvent battleEvent, BattleState battleState);
}