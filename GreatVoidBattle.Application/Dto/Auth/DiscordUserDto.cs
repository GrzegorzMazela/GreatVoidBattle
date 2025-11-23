namespace GreatVoidBattle.Application.Dto.Auth;

public class DiscordUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Discriminator { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool IsAdmin { get; set; }
    public List<string> FractionRoles { get; set; } = new();
}

public class DiscordAuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public DiscordUserDto User { get; set; } = new();
}

public class DiscordLoginUrlDto
{
    public string Url { get; set; } = string.Empty;
}
