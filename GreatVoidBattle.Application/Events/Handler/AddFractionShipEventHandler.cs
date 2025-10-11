using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Factories;
using GreatVoidBattle.Events;

namespace GreatVoidBattle.Application.Events.Handler;

public class AddFractionShipEventHandler : BaseEventHandler<AddFractionShipEvent>
{
    public override Task HandleAsync(AddFractionShipEvent battleEvent, BattleState battleState)
    {
        var ship = ShipFactory.CreateShip(battleEvent);
        battleState.AddFractionShip(battleEvent.FractionId, ship);
        return Task.CompletedTask;
    }
}