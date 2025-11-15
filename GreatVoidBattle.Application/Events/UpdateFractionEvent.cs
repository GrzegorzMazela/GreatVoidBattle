using GreatVoidBattle.Application.Events.Base;

namespace GreatVoidBattle.Application.Events;

public class UpdateFractionEvent : BattleEvent
{
    public string Name { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string FractionColor { get; set; } = string.Empty;
}