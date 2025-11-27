namespace GreatVoidBattle.Application.Dto.GameState;

public class InitializeGameRequest
{
    public string GameName { get; set; } = "Great Void Battle - Main Game";
}

public class InitializeGameResponse
{
    public string SessionId { get; set; } = string.Empty;
    public List<FractionInitInfo> Fractions { get; set; } = new();
}

public class FractionInitInfo
{
    public string FractionId { get; set; } = string.Empty;
    public string FractionName { get; set; } = string.Empty;
    public string DiscordRoleName { get; set; } = string.Empty;
}
