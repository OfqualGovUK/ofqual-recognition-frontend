using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

namespace Ofqual.Recognition.Frontend.Web.Middlewares;

public class FeatureRedirectMiddleware
{
    private readonly RequestDelegate _next;

    public FeatureRedirectMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var featureFlagService = context.RequestServices.GetRequiredService<IFeatureFlagService>();
        var path = context.Request.Path.Value?.ToLowerInvariant();

        if (!featureFlagService.IsFeatureEnabled("Application") &&
            !string.IsNullOrWhiteSpace(path) &&
            (
                path.StartsWith(RouteConstants.ApplicationConstants.TASK_LIST_PATH) ||
                path.StartsWith(RouteConstants.ApplicationConstants.REVIEW_YOUR_TASK_ANSWERS_PATH) ||
                path.StartsWith(RouteConstants.ApplicationConstants.REVIEW_YOUR_APPLICATION_ANSWERS_PATH)
            ))
        {
            context.Response.Redirect(RouteConstants.HomeConstants.HOME_PATH);
            return;
        }

        await _next(context);
    }
}