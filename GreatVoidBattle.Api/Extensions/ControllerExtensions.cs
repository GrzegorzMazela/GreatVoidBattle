using Microsoft.AspNetCore.Mvc;

namespace GreatVoidBattle.Api.Extensions;

public static class ControllerExtensions
{
    public static Guid? GetAuthToken(this ControllerBase controller)
    {
        return controller.HttpContext.Items["AuthToken"] as Guid?;
    }

    public static Guid? GetFractionId(this ControllerBase controller)
    {
        return controller.HttpContext.Items["FractionId"] as Guid?;
    }
}
