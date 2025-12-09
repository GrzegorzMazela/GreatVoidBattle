using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GreatVoidBattle.Core.Domains.Exploration;

/// <summary>
/// Reprezentuje układ gwiezdny w galaktyce - główna jednostka eksploracji
/// </summary>
public class StarSystem
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Nazwa układu gwiezdnego
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Pozycja X na mapie galaktyki
    /// </summary>
    public double X { get; set; }
    
    /// <summary>
    /// Pozycja Y na mapie galaktyki
    /// </summary>
    public double Y { get; set; }
    
    /// <summary>
    /// Typ układu (np. "resources", "habitable", "strategic", "anomaly", "empty")
    /// </summary>
    public string Type { get; set; } = "empty";
    
    /// <summary>
    /// Opis układu widoczny dla admina
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Zasoby dostępne w układzie
    /// </summary>
    public string Resources { get; set; } = string.Empty;
    
    /// <summary>
    /// Informacje o anomaliach w układzie
    /// </summary>
    public string Anomalies { get; set; } = string.Empty;
    
    /// <summary>
    /// ID frakcji kontrolującej układ (null = nikt nie kontroluje)
    /// </summary>
    public string? ControllingFractionId { get; set; }
    
    /// <summary>
    /// Połączenia do sąsiednich układów (ID)
    /// </summary>
    public List<string> ConnectedSystems { get; set; } = new();
    
    /// <summary>
    /// Notatki admina o układzie
    /// </summary>
    public string AdminNotes { get; set; } = string.Empty;
    
    /// <summary>
    /// Sekretne informacje widoczne tylko dla admina
    /// </summary>
    public string SecretInfo { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

