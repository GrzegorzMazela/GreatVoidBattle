using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreatVoidBattle.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DiscordAuthAttribute : Attribute, IAuthorizationFilter
{
    private readonly bool _requireAdmin;
    private readonly string[]? _allowedRoles;

    public DiscordAuthAttribute(bool requireAdmin = false, params string[] allowedRoles)
    {
        _requireAdmin = requireAdmin;
        _allowedRoles = allowedRoles;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (user?.Identity == null || !user.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedObjectResult(new { message = "Unauthorized - Discord authentication required" });
            return;
        }

        if (_requireAdmin)
        {
            var isAdmin = user.Claims.Any(c => 
                c.Type == System.Security.Claims.ClaimTypes.Role && 
                c.Value.Equals("Admin", StringComparison.OrdinalIgnoreCase));

            if (!isAdmin)
            {
                context.Result = new ForbidResult();
                return;
            }
        }

        if (_allowedRoles != null && _allowedRoles.Length > 0)
        {
            var hasRole = user.Claims.Any(c =>
                c.Type == System.Security.Claims.ClaimTypes.Role &&
                _allowedRoles.Contains(c.Value, StringComparer.OrdinalIgnoreCase));

            if (!hasRole)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
