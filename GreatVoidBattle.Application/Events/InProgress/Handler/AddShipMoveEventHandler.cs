using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.InProgress.Handler;

internal class AddShipMoveEventHandler : BaseInProgressEventHandler<AddShipMoveEvent>
{
    public override Task HandleInProgressEventAsync(AddShipMoveEvent battleEvent, BattleState battleState)
    {
        battleState.AddShipMove(battleEvent.FractionId!.Value, battleEvent.ShipId,
            battleEvent.TargetPosition);
        return Task.CompletedTask;
    }
}