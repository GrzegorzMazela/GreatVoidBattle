using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class ShipState
{
    public Guid ShipId { get; private set; }
    public string Name { get; private set; }
    public ShipType Type { get; private set; }

    public Position Position { get; private set; }
    public double Speed { get; private set; }

    public int HitPoints { get; private set; }
    public int Shields { get; private set; }
    public int Armor { get; private set; }
    public int NumberOfModules { get; private set; }

    public int NumberOfMissiles => Modules.Sum(m => m.Slots.Count(x=>x.WeaponType == WeaponType.Missile));
    public int NumberOfPointsDefense => Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.PointDefense));
    public int NumberOfLasers => Modules.Sum(m => m.Slots.Count(x => x.WeaponType == WeaponType.Laser));

    public List<ModuleState> Modules { get; set; } = new();
    public EnergyDistribution Energy { get; set; } = new();

    public ShipStatus Status { get; set; } = ShipStatus.Active;

    // create statict created method
    public static ShipState Create(string name, ShipType type, double positionX, double positionY,
        double speed, int hitPoints, int shields, int armor, int numberOfModules, List<ModuleState> modules)
    {
        if(numberOfModules != modules.Count)
        {
            throw new ArgumentException("Number of modules does not match the actual modules provided.");
        }

        return new ShipState
        {
            ShipId = Guid.NewGuid(),
            Name = name,
            Type = type,
            Position = new Position(positionX, positionY),
            Speed = speed,
            HitPoints = hitPoints,
            Shields = shields,
            Armor = armor,
            Modules = modules,
            NumberOfModules = numberOfModules,
            Energy = new EnergyDistribution(),
            Status = ShipStatus.Active
        };
    }

    public void UpdatePosition(double newX, double newY)
    {
        Position = new Position(newX, newY);
    }
}