using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains;

public class BattleLog
{
    [BsonElement("Logs")]
    private List<LogRecord> _logs { get; set; } = new();

    public IReadOnlyCollection<LogRecord> Logs => _logs.AsReadOnly();

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
}

public record LogRecord(string log, string adminLog);