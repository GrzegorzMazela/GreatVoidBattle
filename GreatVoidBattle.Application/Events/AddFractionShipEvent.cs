using GreatVoidBattle.Application.Events.Base;
using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Events;

public class AddFractionShipEvent : BattleEvent
{
    public string Name { get; set; } = string.Empty;
    public ShipType Type { get; set; }
    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public List<Module> Modules { get; set; } = new();
}

public record Module(List<WeaponType> WeaponTypes);