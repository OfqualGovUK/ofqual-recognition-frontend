using System.Reflection;

using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.Http;

using CorrelationId;
using CorrelationId.DependencyInjection;

using GovUk.Frontend.AspNetCore;

using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Client;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Web.Middlewares;

using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

#region Services

// Add GovUK frontend with new branding
builder.Services.AddGovUkFrontend(options =>
{
    options.Rebrand = true;
});

// Add Controllers with Views
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

// Register Matomo Options
builder.Services.AddSingleton(_ =>
{
    var options = new MatomoModel();
    builder.Configuration.GetSection("Matomo").Bind(options);
    return options;
});

// Configure Serilog logging
builder.Host.UseSerilog((ctx, svc, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(svc)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Environment", ctx.Configuration.GetValue<string>("LogzIo:Environment") ?? "Unknown")
    .Enrich.WithProperty("Assembly", Assembly.GetEntryAssembly()?.GetName()?.Name ?? "Ofqual.Recognition.Frontend")
    .MinimumLevel.Override("CorrelationId", LogEventLevel.Error)
    .WriteTo.Console(
        restrictedToMinimumLevel: ctx.Configuration.GetValue<string>("LogzIo:Environment") == "LOCAL"
            ? LogEventLevel.Verbose
            : LogEventLevel.Error)
    .WriteTo.LogzIoDurableHttp(
        requestUri: ctx.Configuration.GetValue<string>("LogzIo:Uri") ?? string.Empty,
        bufferBaseFileName: "Buffer",
        bufferRollingInterval: BufferRollingInterval.Hour,
        bufferFileSizeLimitBytes: 524288000L,
        retainedBufferFileCountLimit: 12
    )
);

// Add Correlation ID service for tracking requests across logs
builder.Services.AddCorrelationId(opt =>
{
    opt.AddToLoggingScope = true;
    opt.UpdateTraceIdentifier = true;
}).WithTraceIdentifierProvider();

// Enable Correlation ID tracking for incoming requests
builder.Services.AddCorrelationId();

//Add B2C Login
builder.Services.AddOptions();
builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureAdB2C"));

// This needs to be a list of *direct urls* to scopes and not just the names of the scopes!
IEnumerable<string>? initialScopes = builder.Configuration.GetSection("RecognitionApi:Scopes").Get<IEnumerable<string>>();

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);

        if (builder.Configuration.GetValue<bool?>("AzureAdB2C:UseAutomationPolicies") ?? false)
        {
            options.SignUpSignInPolicyId = builder.Configuration.GetValue<string>("AzureAdB2C:SignUpSignInPolicyForAutomationId");
        }

        options.Events.OnRedirectToIdentityProvider += async (context) =>
        {
            var token = context.Properties.Items.FirstOrDefault(x => x.Key == AuthConstants.TokenHintIdentifier).Value;
            if (token != null)
            {
                context.ProtocolMessage.SetParameter(AuthConstants.TokenHintIdentifier, token);
            }

            var redirectUri = builder.Configuration.GetValue<string>("AzureAdB2C:RedirectUri");
            if (!string.IsNullOrEmpty(redirectUri))
            {
                context.ProtocolMessage.RedirectUri = redirectUri + options.CallbackPath.Value;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        };

        options.Events.OnRemoteFailure = context =>
        {
            // Get the error code from the failure message, fallback to query string if needed
            var errorCode = context.Failure?.Message ?? context.Request.Query["error"].ToString();

            // URL encode the error code for safety
            var encodedErrorCode = Uri.EscapeDataString(errorCode ?? string.Empty);

            // Redirect to the error page
            context.Response.Redirect($"/MicrosoftIdentity/OfqualAccount/Error?error={encodedErrorCode}");
            context.HandleResponse();
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToIdentityProviderForSignOut += async (context) =>
        {
            var id_token_hint = context.Properties.Items.FirstOrDefault(x => x.Key == "id_token_hint").Value;
            if (id_token_hint != null)
            {
                // Send parameter to authentication request
                context.ProtocolMessage.SetParameter("id_token_hint", id_token_hint);
            }

            await Task.CompletedTask.ConfigureAwait(false);
        };

        options.SaveTokens = true;
    })
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddDistributedTokenCaches();

// Configure HttpClient for API calls
builder.Services.AddHttpClient("RecognitionCitizen", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RecognitionApi:BaseUrl"]!);
});

// Register in-memory caching
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<IMemoryCacheService, MemoryCacheService>();

// Register session management
builder.Services.AddSession(options =>
{
    options.Cookie.Name = CookieConstants.SessionCookieName;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(20);
    options.Cookie.IsEssential = true;
});

// Register Helpdesk contact configuration
builder.Services.Configure<HelpDeskContact>(builder.Configuration.GetSection("HelpdeskContact"));
builder.Services.AddSingleton<HelpDeskContact>(sp =>
    sp.GetRequiredService<IOptions<HelpDeskContact>>().Value);

// Register essential services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRecognitionCitizenClient, RecognitionCitizenClient>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();
builder.Services.AddScoped<IPreEngagementService, PreEngagementService>();
builder.Services.AddScoped<IAttachmentService, AttachmentService>();

#endregion

var app = builder.Build();

#region Middleware
app.UseGovUkFrontend();

// Configure middleware and request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseCorrelationId();
app.UseSession();
app.UseHttpsRedirection();
app.UseStatusCodePagesWithReExecute("/Error/{0}");
app.UseStaticFiles();
app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();

app.UseAuthorization();
app.UseMiddleware<FeatureRedirectMiddleware>();
app.UseMiddleware<RedirectReadOnlyMiddleware>();

// Configure route mapping
app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}"
);

#endregion

app.Run();