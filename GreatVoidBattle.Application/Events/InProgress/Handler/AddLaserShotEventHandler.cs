using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.InProgress.Handler;

public class AddLaserShotEventHandler : BaseInProgressEventHandler<AddLaserShotEvent>
{
    public override Task HandleInProgressEventAsync(AddLaserShotEvent battleEvent, BattleState battleState)
    {
        battleState.AddLaserShot(battleEvent.FractionId!.Value, battleEvent.ShipId,
            battleEvent.TargetFractionId, battleEvent.TargetShipId);
        return Task.CompletedTask;
    }
}