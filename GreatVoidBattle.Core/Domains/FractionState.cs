using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class FractionState
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public IReadOnlyList<ShipState> Ships => _ships.AsReadOnly();
    private List<ShipState> _ships { get; set; } = new();

    public bool IsDefeated => _ships.All(s => s.Status is ShipStatus.Destroyed or ShipStatus.Retreated);

    public static FractionState CreateNew(string fractionName)
    {
        return new FractionState
        {
            FractionId = Guid.NewGuid(),
            FractionName = fractionName,
            _ships = new List<ShipState>()
        };
    }

    public void AddShip(ShipState ship)
    {
        _ships.Add(ship);
    }
}