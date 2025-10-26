using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler;

public class AddFractionEventHandler : BasePreparationEventHandler<AddFractionEvent>
{
    public override Task HandlePreparationEventAsync(AddFractionEvent battleEvent, BattleState battleState)
    {
        battleState.AddFraction(FractionState.CreateNew(battleEvent.Name, battleState.BattleLog));
        return Task.CompletedTask;
    }
}