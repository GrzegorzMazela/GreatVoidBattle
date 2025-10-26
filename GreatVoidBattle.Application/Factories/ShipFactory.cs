using GreatVoidBattle.Core.Domains;
using GreatVoidBattle.Core.Domains.Enums;
using GreatVoidBattle.Events;

namespace GreatVoidBattle.Core.Factories;

public static class ShipFactory
{
    public static ShipState CreateShip(AddFractionShipEvent addShipEvent, BattleLog battleLog)
    => addShipEvent.Type switch
    {
        ShipType.Corvette => CreateShipState(addShipEvent, speed: 10, hitPoints: 50, shields: 25, armor: 25, 1, battleLog),
        ShipType.Destroyer => CreateShipState(addShipEvent, speed: 8, hitPoints: 100, shields: 50, armor: 50, 2, battleLog),
        ShipType.Cruiser => CreateShipState(addShipEvent, speed: 6, hitPoints: 200, shields: 100, armor: 100, 4, battleLog),
        ShipType.Battleship => CreateShipState(addShipEvent, speed: 5, hitPoints: 400, shields: 200, armor: 200, 8, battleLog),
        ShipType.SuperBattleship => CreateShipState(addShipEvent, speed: 5, hitPoints: 600, shields: 300, armor: 300, 12, battleLog),
        ShipType.OrbitalFort => CreateShipState(addShipEvent, speed: 0, hitPoints: 100, shields: 50, armor: 50, 2, battleLog),
        _ => throw new ArgumentOutOfRangeException()
    };

    private static ShipState CreateShipState(AddFractionShipEvent addShipEvent, int speed, int hitPoints,
        int shields, int armor, int numberOfModules, BattleLog battleLog)
    {
        return ShipState.Create(addShipEvent.FractionId!.Value, addShipEvent.Name, addShipEvent.Type, addShipEvent.PositionX, addShipEvent.PositionY,
            speed: speed, hitPoints: hitPoints, shields: shields, armor: armor, numberOfModules: numberOfModules,
            addShipEvent.Modules.Select(m => ModuleState.Create(m.WeaponTypes.Select(wt => SystemSlot.Create(wt)).ToList())).ToList(), battleLog);
    }
}