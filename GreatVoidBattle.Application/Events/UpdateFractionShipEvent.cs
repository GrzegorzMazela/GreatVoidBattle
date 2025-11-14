using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Application.Events;

public class UpdateFractionShipEvent : BattleEvent
{
    public Guid ShipId { get; set; }
    public string Name { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public ShipType Type { get; set; }
    public List<Module> Modules { get; set; }
}