using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler;

public class AddFractionEventHandler : BaseEventHandler<AddFractionEvent>
{
    public override Task HandleAsync(AddFractionEvent battleEvent, BattleState battleState)
    {
        battleState.AddFraction(FractionState.CreateNew(battleEvent.Name));
        return Task.CompletedTask;
    }
}