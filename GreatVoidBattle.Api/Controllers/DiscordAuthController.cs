using GreatVoidBattle.Application.Dto.Auth;
using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text;

namespace GreatVoidBattle.Api.Controllers;

[ApiController]
[Route("api/auth/discord")]
public class DiscordAuthController : ControllerBase
{
    private readonly IDiscordService _discordService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DiscordAuthController> _logger;
    private readonly IMemoryCache _cache;

    public DiscordAuthController(
        IDiscordService discordService,
        IConfiguration configuration,
        ILogger<DiscordAuthController> logger,
        IMemoryCache cache)
    {
        _discordService = discordService;
        _configuration = configuration;
        _logger = logger;
        _cache = cache;
    }

    [HttpGet("login")]
    public IActionResult GetLoginUrl()
    {
        var state = GenerateState();
        _cache.Set($"discord_state_{state}", state, TimeSpan.FromMinutes(5));
        var url = _discordService.GetAuthorizationUrl(state);
        return Ok(new DiscordLoginUrlDto { Url = url });
    }

    [HttpGet("callback")]
    public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
    {
        // Weryfikuj state z cache
        if (!_cache.TryGetValue($"discord_state_{state}", out string? savedState) ||
            string.IsNullOrEmpty(savedState) ||
            savedState != state)
        {
            _logger.LogWarning("Invalid state parameter. State: {State}", state);
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            return Redirect($"{frontendUrl}/login?error=invalid_state");
        }

        // Usuń użyty state
        _cache.Remove($"discord_state_{state}");

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest(new { message = "Authorization code not provided" });
        }

        try
        {
            // Wymień code na access token
            var tokenResponse = await _discordService.ExchangeCodeForTokenAsync(code);
            if (tokenResponse == null)
            {
                return BadRequest(new { message = "Failed to exchange code for token" });
            }

            // Pobierz informacje o użytkowniku
            var user = await _discordService.GetUserInfoAsync(tokenResponse.Access_Token);
            if (user == null)
            {
                return BadRequest(new { message = "Failed to fetch user information" });
            }

            // Pobierz role użytkownika z serwera Discord
            var guildMember = (await _discordService.GetGuildMemberAsync(user.Id, tokenResponse.Access_Token))!
                .FirstOrDefault();
            if (guildMember is null)
            {
                return BadRequest(new { message = "User is not a member of the guild" });
            }

            // Mapuj role Discord na role aplikacji
            var userDto = await MapDiscordUserToDto(user, guildMember);

            var response = new DiscordAuthResponseDto
            {
                AccessToken = tokenResponse.Access_Token,
                User = userDto
            };

            // Przekieruj do frontendu z tokenem i danymi użytkownika
            var frontendUrl = _configuration["FrontendUrl"] ?? "http://localhost:5173";
            var redirectUrl = $"{frontendUrl}/auth/discord/callback?" +
                $"token={tokenResponse.Access_Token}&" +
                $"userId={user.Id}&" +
                $"username={Uri.EscapeDataString(user.Username)}&" +
                $"discriminator={user.Discriminator}&" +
                $"avatar={Uri.EscapeDataString(user.Avatar ?? "")}&" +
                $"email={Uri.EscapeDataString(user.Email ?? "")}";

            return Redirect(redirectUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Discord OAuth callback");
            return StatusCode(500, new { message = "Internal server error during authentication" });
        }
    }

    [HttpGet("validate")]
    public async Task<IActionResult> ValidateToken([FromHeader(Name = "X-Discord-Token")] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Ok(new { valid = false, message = "Token not provided" });
        }

        try
        {
            var user = await _discordService.GetUserInfoAsync(token);
            if (user == null)
            {
                return Ok(new { valid = false, message = "Invalid token" });
            }

            var guildMember = (await _discordService.GetGuildMemberAsync(user.Id, token))!.FirstOrDefault();
            if (guildMember is null)
            {
                return Ok(new { valid = false, message = "User is not a member of the guild" });
            }

            var userDto = await MapDiscordUserToDto(user, guildMember);

            return Ok(new
            {
                valid = true,
                user = userDto,
                roles = userDto.Roles
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Discord token");
            return Ok(new { valid = false, message = "Token validation failed" });
        }
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetCurrentUser([FromHeader(Name = "X-Discord-Token")] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = "Token not provided" });
        }

        try
        {
            var user = await _discordService.GetUserInfoAsync(token);
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var guildMember = (await _discordService.GetGuildMemberAsync(user.Id, token))!
                .FirstOrDefault();

            if (guildMember is null)
            {
                return Unauthorized(new { message = "User is not a member of the guild" });
            }

            var userDto = await MapDiscordUserToDto(user, guildMember);

            return Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current user");
            return StatusCode(500, new { message = "Failed to fetch user information" });
        }
    }

    private async Task<DiscordUserDto> MapDiscordUserToDto(DiscordUser user, DiscordGuildMember discordGuildMember)
    {
        // Log role IDs for debugging
        _logger.LogInformation("User {UserId} has role IDs: {RoleIds}", user.Id, string.Join(", ", discordGuildMember.Roles));

        // Pobierz mapowanie ID ról na nazwy
        var roleMapping = _configuration.GetSection("Discord:RoleMapping")
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        var adminRoleId = _configuration["Discord:AdminRoleId"] ?? string.Empty;
        var adminRoleName = _configuration["Discord:AdminRoleName"] ?? "Admin";
        var fractionRoles = _configuration.GetSection("Discord:FractionRoles")
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);

        _logger.LogInformation("Admin Role ID: {AdminRoleId}", adminRoleId);
        _logger.LogInformation("Role Mapping: {Mapping}", string.Join(", ", roleMapping.Select(kvp => $"{kvp.Key}={kvp.Value}")));

        // Mapuj ID ról na nazwy
        var roleNames = discordGuildMember.Roles
            .Select(roleId => roleMapping.TryGetValue(roleId, out var name) ? name : null)
            .Where(name => !string.IsNullOrEmpty(name))
            .Cast<string>()
            .ToList();

        _logger.LogInformation("Mapped role names: {RoleNames}", string.Join(", ", roleNames));

        // Sprawdź czy użytkownik ma rolę admina (po ID lub nazwie)
        var isAdmin = discordGuildMember.Roles.Contains(adminRoleId) ||
                      roleNames.Any(r => r != null && r.Equals(adminRoleName, StringComparison.OrdinalIgnoreCase));

        _logger.LogInformation("User {UserId} isAdmin: {IsAdmin}", user.Id, isAdmin);

        // Zbierz wszystkie role frakcyjne użytkownika
        var userFractionRoles = new List<string>();
        foreach (var fr in fractionRoles.Values)
        {
            if (roleNames.Any(r => r.Equals(fr, StringComparison.OrdinalIgnoreCase)))
            {
                userFractionRoles.Add(fr);
            }
        }

        _logger.LogInformation("User {UserId} fraction roles: {FractionRoles}", user.Id, string.Join(", ", userFractionRoles));

        return new DiscordUserDto
        {
            Id = user.Id,
            Username = discordGuildMember.Nick,
            Discriminator = user.Discriminator,
            Avatar = user.Avatar,
            Email = user.Email,
            Roles = roleNames,
            IsAdmin = isAdmin,
            FractionRoles = userFractionRoles
        };
    }

    private string GenerateState()
    {
        return Guid.NewGuid().ToString();
    }
}