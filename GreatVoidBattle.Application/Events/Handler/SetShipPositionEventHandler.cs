using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Application.Exceptions;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler;

public class SetShipPositionEventHandler : BasePreparationEventHandler<SetShipPositionEvent>
{
    public override Task HandlePreparationEventAsync(SetShipPositionEvent battleEvent, BattleState battleState)
    {
        battleState.SetNewShipPosition(battleEvent.FractionId!.Value, battleEvent.ShipId, battleEvent.NewPositionX, battleEvent.NewPositionY);
        return Task.CompletedTask;
    }
}
