using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Factories;

namespace GreatVoidBattle.Application.Events.Handler;

public class AddFractionShipEventHandler : BaseEventHandler<AddFractionShipEvent>
{
    public override Task HandleAsync(AddFractionShipEvent battleEvent, BattleState battleState)
    {
        var ship = ShipFactory.CreateShip(battleEvent, battleState.BattleLog);
        battleState.AddFractionShip(battleEvent.FractionId!.Value, ship);
        return Task.CompletedTask;
    }
}