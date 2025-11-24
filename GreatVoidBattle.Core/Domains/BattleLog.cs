using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains;

public class BattleLog
{
    [BsonElement("Logs")]
    private List<LogRecord> _logs { get; set; } = new();

    public IReadOnlyCollection<LogRecord> Logs => _logs.AsReadOnly();

    [BsonElement("TurnLogs")]
    private Dictionary<string, List<TurnLogEntry>> _turnLogs { get; set; } = new();

    public IReadOnlyDictionary<string, List<TurnLogEntry>> TurnLogs => _turnLogs.AsReadOnly();

    /// <summary>
    /// Adds a log entry to the battle log.
    /// </summary>
    /// <param name="log"></param>
    /// <param name="adminLog"></param>
    public void AddLog(string log, string adminLog)
    {
        _logs.Add(new LogRecord(log, adminLog));
    }

    /// <summary>
    /// Adds a log entry for damage taken by a target.
    /// </summary>
    /// <param name="targetId"></param>
    /// <param name="targetName"></param>
    /// <param name="damage"></param>
    /// <param name="accuracy"></param>
    /// <param name="shotAccuracy"></param>
    public void AddTakeDamageLog(Guid targetId, string targetName, int damage, int accuracy, int shotAccuracy)
    {
        var log = $"{targetName} takes {damage} damage.";
        var adminLog = $"{targetId} : {targetName} takes {damage} damage. Accuracy: {accuracy} Shot Accuracy: {shotAccuracy}";
        _logs.Add(new LogRecord(log, adminLog));
    }

    public void AddMissedLog(Guid targetId, string targetName, int accuracy, int shotAccuracy)
    {
        var log = $"The shot at ship {targetName} missed";
        var adminLog = $"The shot at ship {targetId} : {targetName} missed. Accuracy: {accuracy} Shot Accuracy: {shotAccuracy}";
        _logs.Add(new LogRecord(log, adminLog));
    }

    public void ShipIsDestroyLog(Guid targetId, string targetName)
    {
        var log = $"{targetName} is destroyed.";
        var adminLog = $"{targetId} : {targetName} is destroyed.";
        _logs.Add(new LogRecord(log, adminLog));
    }

    /// <summary>
    /// Adds a turn-specific log entry for a fraction.
    /// </summary>
    public void AddTurnLog(int turnNumber, TurnLogEntry entry)
    {
        var key = turnNumber.ToString();
        if (!_turnLogs.ContainsKey(key))
        {
            _turnLogs[key] = new List<TurnLogEntry>();
        }
        _turnLogs[key].Add(entry);
    }

    /// <summary>
    /// Gets turn logs for a specific fraction and turn.
    /// </summary>
    public List<TurnLogEntry> GetTurnLogsForFraction(int turnNumber, Guid fractionId)
    {
        var key = turnNumber.ToString();
        if (!_turnLogs.ContainsKey(key))
        {
            return new List<TurnLogEntry>();
        }

        return _turnLogs[key]
            .Where(log => log.FractionId == fractionId || log.TargetFractionId == fractionId)
            .ToList();
    }

    /// <summary>
    /// Gets all turn logs for a specific turn (admin view).
    /// </summary>
    public List<TurnLogEntry> GetTurnLogs(int turnNumber)
    {
        var key = turnNumber.ToString();
        if (!_turnLogs.ContainsKey(key))
        {
            return new List<TurnLogEntry>();
        }

        return _turnLogs[key];
    }
}

public record LogRecord(string log, string adminLog);

public class TurnLogEntry
{
    public TurnLogType Type { get; set; }
    public Guid FractionId { get; set; }
    public string FractionName { get; set; } = string.Empty;
    public Guid? TargetFractionId { get; set; }
    public string? TargetFractionName { get; set; }
    public Guid ShipId { get; set; }
    public string ShipName { get; set; } = string.Empty;
    public Guid? TargetShipId { get; set; }
    public string? TargetShipName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AdminLog { get; set; }
}

public enum TurnLogType
{
    ShipMove,
    LaserHit,
    LaserMiss,
    MissileFired,
    MissileHit,
    MissileMiss,
    MissileIntercepted,
    ShipDestroyed,
    DamageDealt,
    DamageReceived
}