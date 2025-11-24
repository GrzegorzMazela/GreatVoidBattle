using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.InProgress.Handler;

internal class AddMissileShotEventHandler : BaseInProgressEventHandler<AddMissileShotEvent>
{
    public override Task HandleInProgressEventAsync(AddMissileShotEvent battleEvent, BattleState battleState)
    {
        battleState.AddMissileShot(battleEvent.FractionId!.Value, battleEvent.ShipId,
            battleEvent.TargetFractionId, battleEvent.TargetShipId);
        return Task.CompletedTask;
    }
}