using GreatVoidBattle.Application.Factories;

namespace GreatVoidBattle.Api.Middleware;

public class FractionAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public FractionAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, BattleManagerFactory battleManagerFactory)
    {
        // Sprawdź czy request wymaga autoryzacji frakcji
        var endpoint = context.GetEndpoint();
        var hasFractionAuth = endpoint?.Metadata.GetMetadata<Attributes.FractionAuthAttribute>() != null;

        if (hasFractionAuth)
        {
            var authToken = context.Items["AuthToken"] as Guid?;
            var fractionId = context.Items["FractionId"] as Guid?;

            if (authToken.HasValue && fractionId.HasValue)
            {
                // Pobierz battleId z route
                if (context.Request.RouteValues.TryGetValue("battleId", out var battleIdObj) 
                    && Guid.TryParse(battleIdObj?.ToString(), out var battleId))
                {
                    var battleState = await battleManagerFactory.GetBattleState(battleId);
                    var fraction = battleState.Fractions.FirstOrDefault(f => f.FractionId == fractionId.Value);

                    if (fraction == null || fraction.AuthToken != authToken.Value)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        await context.Response.WriteAsJsonAsync(new { error = "Unauthorized access to this fraction" });
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}

public static class FractionAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseFractionAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<FractionAuthorizationMiddleware>();
    }
}
