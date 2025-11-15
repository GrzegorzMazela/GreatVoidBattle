using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GreatVoidBattle.Api.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class FractionAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Auth-Token", out var authTokenHeader))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Missing X-Auth-Token header" });
            return;
        }

        if (!Guid.TryParse(authTokenHeader, out var authToken))
        {
            context.Result = new UnauthorizedObjectResult(new { error = "Invalid X-Auth-Token format" });
            return;
        }

        // Zapisz AuthToken w HttpContext.Items aby był dostępny w kontrolerze
        context.HttpContext.Items["AuthToken"] = authToken;

        // Jeśli jest fractionId w route, zapisz również
        if (context.RouteData.Values.TryGetValue("fractionId", out var fractionIdObj) 
            && Guid.TryParse(fractionIdObj?.ToString(), out var fractionId))
        {
            context.HttpContext.Items["FractionId"] = fractionId;
        }

        await next();
    }
}
