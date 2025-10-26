using GreatVoidBattle.Core.Domains.Enums;

namespace GreatVoidBattle.Core.Domains;

public class FractionState
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public IReadOnlyList<ShipState> Ships => _ships.AsReadOnly();
    private List<ShipState> _ships { get; set; } = new();
    private BattleLog _battleLog;

    public bool IsDefeated => _ships.All(s => s.Status is ShipStatus.Destroyed or ShipStatus.Retreated);

    public static FractionState CreateNew(string fractionName, BattleLog battleLog)
    {
        return new FractionState
        {
            FractionId = Guid.NewGuid(),
            FractionName = fractionName,
            _ships = new List<ShipState>(),
            _battleLog = battleLog
        };
    }

    public void AddShip(ShipState ship)
    {
        _ships.Add(ship);
    }

    public ShipState GetShip(Guid shipId)
    {
        var ship = _ships.FirstOrDefault(s => s.ShipId == shipId);
        if (ship is null)
        {
            throw new InvalidOperationException($"Ship with ID {shipId} not exists in the fraction.");
        }
        return ship;
    }

    public void RemoveShip(ShipState ship)
    {
        _ships.Remove(ship);
    }
}