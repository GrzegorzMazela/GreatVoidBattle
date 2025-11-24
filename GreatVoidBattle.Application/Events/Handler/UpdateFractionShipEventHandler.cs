using GreatVoidBattle.Application.Events.Handler.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.Handler;

public class UpdateFractionShipEventHandler : BaseEventHandler<UpdateFractionShipEvent>
{
    public override Task HandleAsync(UpdateFractionShipEvent battleEvent, BattleState battleState)
    {
        var ship = battleState.GetShip(battleEvent.ShipId);
        if (ship is null)
        {
            throw new InvalidOperationException($"Ship with ID {battleEvent.ShipId} not exists in the fraction.");
        }

        ship.UpdateName(battleEvent.Name);
        ship.UpdatePosition(battleEvent.PositionX, battleEvent.PositionY);
        ship.UpdateType(battleEvent.Type,
            battleEvent.Modules.Select(m => ModuleState.Create(m.WeaponTypes.Select(wt => SystemSlot.Create(wt)).ToList())).ToList());
        return Task.CompletedTask;
    }
}