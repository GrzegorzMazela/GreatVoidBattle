namespace GreatVoidBattle.Core.Domains;

public class BattleState
{
    public Guid BattleId { get; set; }
    public string BattleName { get; set; } = string.Empty;
    public int TurnNumber { get; set; }

    public List<FractionState> Fractions { get; set; } = new();

    public DateTime LastUpdated { get; set; }

    public static BattleState CreateNew(string battleName)
    {
        return new BattleState
        {
            BattleId = Guid.NewGuid(),
            BattleName = battleName,
            TurnNumber = 0,
            Fractions = new List<FractionState>(),
            LastUpdated = DateTime.UtcNow
        };
    }

    public void AddFraction(FractionState fraction)
    {
        if (Fractions.Any(f => f.FractionId == fraction.FractionId))
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} already exists in the battle.");
        }
        Fractions.Add(fraction);
        LastUpdated = DateTime.UtcNow;
    }

    public void AddFractionShip(Guid fractionId, ShipState shipState)
    {
        var fraction = Fractions.FirstOrDefault(f => f.FractionId == fractionId);

        if (fraction is null)
        {
            throw new InvalidOperationException($"Fraction with ID {fraction.FractionId} not exists in the battle.");
        }
        fraction.AddShip(shipState);
        LastUpdated = DateTime.UtcNow;
    }
}