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
        if (battleEvent.LaserMaxRange.HasValue || battleEvent.LaserDamage.HasValue || battleEvent.MissileMaxRange.HasValue
            || battleEvent.MissileEffectiveRange.HasValue || battleEvent.MissileDamage.HasValue || battleEvent.MissileSpeed.HasValue)
        {
            ship.UpdateWeaponStats(
                battleEvent.LaserMaxRange ?? 0,
                battleEvent.LaserDamage ?? 0,
                battleEvent.MissileMaxRange ?? 0,
                battleEvent.MissileEffectiveRange ?? 0,
                battleEvent.MissileDamage ?? 0,
                battleEvent.MissileSpeed ?? 0);
        }
        return Task.CompletedTask;
    }
}