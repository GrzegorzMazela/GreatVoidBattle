using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Core.Domains;

namespace GreatVoidBattle.Application.Events.InProgress;

public class AddShipMoveEvent : BattleEvent
{
    public Guid ShipId { get; set; }

    public Position TargetPosition { get; set; }
}