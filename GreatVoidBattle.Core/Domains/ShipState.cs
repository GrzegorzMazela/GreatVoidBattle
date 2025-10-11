using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class ShipState
{
    public Guid ShipId { get; set; }
    public string Name { get; set; }
    public ShipType Type { get; set; }

    public double PositionX { get; set; }
    public double PositionY { get; set; }
    public double Speed { get; set; }

    public int HitPoints { get; set; }
    public int Shields { get; set; }
    public int Armor { get; set; }

    public List<ModuleState> Modules { get; set; } = new();
    public EnergyDistribution Energy { get; set; } = new();

    public ShipStatus Status { get; set; } = ShipStatus.Active;

    // create statict created method
    public static ShipState Create(string name, ShipType type, double positionX, double positionY,
        double speed, int hitPoints, int shields, int armor, List<ModuleState> modules)
    {
        return new ShipState
        {
            ShipId = Guid.NewGuid(),
            Name = name,
            Type = type,
            PositionX = positionX,
            PositionY = positionY,
            Speed = speed,
            HitPoints = hitPoints,
            Shields = shields,
            Armor = armor,
            Modules = modules,
            Energy = new EnergyDistribution(),
            Status = ShipStatus.Active
        };
    }
}