using GreatVoidBattle.Application.Events.Base;

namespace GreatVoidBattle.Application.Events
{
    public class SetShipPositionEvent : BattleEvent
    {
        public Guid ShipId { get; set; }
        public double NewPositionX { get; set; }
        public double NewPositionY { get; set; }
    }
}
