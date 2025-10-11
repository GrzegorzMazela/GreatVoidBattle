using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;


namespace GreatVoidBattle.Application.Events.Handler;

public class StartBattleEventHandler : BasePreparationEventHandler<StartBattleEvent>
{
    public override Task HandlePreparationEventAsync(StartBattleEvent battleEvent, BattleState battleState)
    {
        battleState.StartBattle();
        return Task.CompletedTask;
    }
}
