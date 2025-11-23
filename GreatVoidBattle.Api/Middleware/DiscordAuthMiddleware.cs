using GreatVoidBattle.Application.Services;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace GreatVoidBattle.Api.Middleware;

public class DiscordAuthMiddleware
{
    private readonly RequestDelegate _next;

    public DiscordAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IDiscordService discordService)
    {
        // Sprawdź czy request ma Discord token
        if (context.Request.Headers.TryGetValue("X-Discord-Token", out var token) && !string.IsNullOrEmpty(token))
        {
            try
            {
                var user = await discordService.GetUserInfoAsync(token!);
                if (user != null)
                {
                    var guildMembers = await discordService.GetGuildMemberAsync(user.Id, token!);
                    var roles = guildMembers?.FirstOrDefault()?.Roles ?? new List<string>();

                    // Dodaj claims do HttpContext
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id),
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
                    };

                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var identity = new ClaimsIdentity(claims, "Discord");
                    context.User = new ClaimsPrincipal(identity);
                }
            }
            catch
            {
                // Token nieprawidłowy - kontynuuj bez uwierzytelnienia
            }
        }

        await _next(context);
    }
}
