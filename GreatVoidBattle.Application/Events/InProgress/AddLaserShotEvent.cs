using GreatVoidBattle.Application.Events.Base;

namespace GreatVoidBattle.Application.Events.InProgress;

public class AddLaserShotEvent : BattleEvent
{
    public Guid ShipId { get; set; }
    public Guid TargetFractionId { get; set; }
    public Guid TargetShipId { get; set; }
}