namespace GreatVoidBattle.Application.Dto.Exploration;

/// <summary>
/// DTO układu gwiezdnego - widok podstawowy
/// </summary>
public class StarSystemDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string Type { get; set; } = string.Empty;
    public string? ControllingFractionId { get; set; }
    public List<string> ConnectedSystems { get; set; } = new();
}

/// <summary>
/// DTO układu gwiezdnego - widok admina (pełne informacje)
/// </summary>
public class StarSystemAdminDto : StarSystemDto
{
    public string Description { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    public string Anomalies { get; set; } = string.Empty;
    public string AdminNotes { get; set; } = string.Empty;
    public string SecretInfo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// DTO do aktualizacji układu gwiezdnego
/// </summary>
public class UpdateStarSystemDto
{
    public string? Name { get; set; }
    public double? X { get; set; }
    public double? Y { get; set; }
    public string? Type { get; set; }
    public string? Description { get; set; }
    public string? Resources { get; set; }
    public string? Anomalies { get; set; }
    public string? ControllingFractionId { get; set; }
    public List<string>? ConnectedSystems { get; set; }
    public string? AdminNotes { get; set; }
    public string? SecretInfo { get; set; }
}

/// <summary>
/// DTO do tworzenia nowego układu gwiezdnego
/// </summary>
public class CreateStarSystemDto
{
    public string Name { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public string Type { get; set; } = "empty";
    public string Description { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    public string Anomalies { get; set; } = string.Empty;
    public string? ControllingFractionId { get; set; }
    public List<string> ConnectedSystems { get; set; } = new();
}

