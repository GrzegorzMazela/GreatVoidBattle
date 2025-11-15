using GreatVoidBattle.Core.Domains.Enums;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains;

public class FractionState
{
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public string PlayerName { get; set; } = string.Empty;
    public string FractionColor { get; set; } = string.Empty;
    public Guid AuthToken { get; set; }
    public bool TurnFinished { get; set; } = false;
    public IReadOnlyList<ShipState> Ships => _ships.AsReadOnly();

    [BsonElement("Ships")]
    private List<ShipState> _ships { get; set; } = new();

    public bool IsDefeated => _ships.All(s => s.Status is ShipStatus.Destroyed or ShipStatus.Retreated);

    public static FractionState CreateNew(string fractionName, string playerName, string fractionColor)
    {
        return new FractionState
        {
            FractionId = Guid.NewGuid(),
            FractionName = fractionName,
            PlayerName = playerName,
            FractionColor = fractionColor,
            AuthToken = Guid.NewGuid(),
            _ships = new List<ShipState>()
        };
    }

    public void AddShip(ShipState ship)
    {
        _ships.Add(ship);
    }

    public void UpdateShip(ShipState updatedShip)
    {
        var existingShip = GetShip(updatedShip.ShipId);
        var index = _ships.IndexOf(existingShip);
        _ships[index] = updatedShip;
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