using GreatVoidBattle.Application.Events.Base;

namespace GreatVoidBattle.Application.Events;

public class AddFractionEvent : BattleEvent
{
    public string Name { get; set; }
}