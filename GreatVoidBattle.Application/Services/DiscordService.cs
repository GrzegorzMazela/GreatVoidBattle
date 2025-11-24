using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace GreatVoidBattle.Application.Services;

public interface IDiscordService
{
    string GetAuthorizationUrl(string state);

    Task<DiscordTokenResponse?> ExchangeCodeForTokenAsync(string code);

    Task<DiscordUser?> GetUserInfoAsync(string accessToken);

    Task<List<DiscordGuildMember>?> GetGuildMemberAsync(string userId, string accessToken);

    Task<List<DiscordRole>?> GetGuildRolesAsync(string botToken);
}

public class DiscordService : IDiscordService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _redirectUri;
    private readonly string _guildId;

    public DiscordService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _clientId = configuration["Discord:ClientId"] ?? throw new Exception("Discord:ClientId not configured");
        _clientSecret = configuration["Discord:ClientSecret"] ?? throw new Exception("Discord:ClientSecret not configured");
        _redirectUri = configuration["Discord:RedirectUri"] ?? throw new Exception("Discord:RedirectUri not configured");
        _guildId = configuration["Discord:GuildId"] ?? throw new Exception("Discord:GuildId not configured");

        _httpClient.BaseAddress = new Uri("https://discord.com/api/v10/");
    }

    public string GetAuthorizationUrl(string state)
    {
        var scopes = "identify email guilds guilds.members.read";
        return $"https://discord.com/api/oauth2/authorize?" +
               $"client_id={_clientId}&" +
               $"redirect_uri={Uri.EscapeDataString(_redirectUri)}&" +
               $"response_type=code&" +
               $"scope={Uri.EscapeDataString(scopes)}&" +
               $"state={state}";
    }

    public async Task<DiscordTokenResponse?> ExchangeCodeForTokenAsync(string code)
    {
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["client_id"] = _clientId,
            ["client_secret"] = _clientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = _redirectUri
        });

        var response = await _httpClient.PostAsync("oauth2/token", content);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordTokenResponse>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<DiscordUser?> GetUserInfoAsync(string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "users/@me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<DiscordUser>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public async Task<List<DiscordGuildMember>?> GetGuildMemberAsync(string userId, string accessToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"users/@me/guilds/{_guildId}/member");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Discord API Response Status: {response.StatusCode}");
        Console.WriteLine($"Discord API Response Body: {json}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to fetch guild member. Status: {response.StatusCode}, Response: {json}");
            return null;
        }

        var member = JsonSerializer.Deserialize<DiscordGuildMember>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        Console.WriteLine($"Deserialized member roles count: {member?.Roles?.Count ?? 0}");
        Console.WriteLine($"Deserialized member roles: {string.Join(", ", member?.Roles ?? new List<string>())}");

        return member != null ? new List<DiscordGuildMember> { member } : null;
    }

    public async Task<List<DiscordRole>?> GetGuildRolesAsync(string botToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", botToken);

        var response = await _httpClient.GetAsync($"guilds/{_guildId}/roles");
        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DiscordRole>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
}

public class DiscordTokenResponse
{
    public string Access_Token { get; set; } = string.Empty;
    public string Token_Type { get; set; } = string.Empty;
    public int Expires_In { get; set; }
    public string Refresh_Token { get; set; } = string.Empty;
    public string Scope { get; set; } = string.Empty;
}

public class DiscordUser
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Discriminator { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public bool? Verified { get; set; }
}

public class DiscordGuildMember
{
    public List<string> Roles { get; set; } = new();
    public string Nick { get; set; } = string.Empty;
    public string Joined_At { get; set; } = string.Empty;
}

public class DiscordRole
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Color { get; set; }
    public int Position { get; set; }
}