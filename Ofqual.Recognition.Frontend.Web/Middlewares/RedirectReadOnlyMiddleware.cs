using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Core.Constants;
using Microsoft.AspNetCore.Http.Features;
using Ofqual.Recognition.Frontend.Core.Attributes;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Core.Models;

namespace Ofqual.Recognition.Frontend.Web.Middlewares;

public class RedirectReadOnlyMiddleware
{
    private readonly RequestDelegate _next;

    public RedirectReadOnlyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ISessionService sessionService)
    {
        var endpoint = context.Features.Get<IEndpointFeature>()?.Endpoint; // Get the endpoint being called
        var attribute = endpoint?.Metadata.GetMetadata<RedirectReadOnly>(); // Get the attribute if available

        if (attribute != null)
        {
            Application? application = sessionService.GetFromSession<Application>(SessionKeys.Application);
            if (application != null && application.Submitted) {
                context.Response.Redirect(RouteConstants.ApplicationConstants.TASK_LIST_PATH); // Redirect to task list if attribute is present and already submitted
            }
        }

        await _next(context);
    }
}