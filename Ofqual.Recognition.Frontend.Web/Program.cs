using CorrelationId.DependencyInjection;
using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Serilog.Sinks.Http;
using System.Reflection;
using Serilog.Events;
using CorrelationId;
using Serilog;
using Ofqual.Recognition.Frontend.Core.Constants;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Client.Interfaces;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Infrastructure.Client;
using Ofqual.Recognition.Frontend.Web.Middlewares;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

#region Services

// Add GovUK frontend
builder.Services.AddGovUkFrontend();

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

builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.Events ??= new OpenIdConnectEvents();
        options.Events.OnRedirectToIdentityProvider += async (context) =>
        {            
            var token = context.Properties.Items.FirstOrDefault(x => x.Key == AuthConstants.TokenHintIdentifier).Value;
            if (token != null)
                context.ProtocolMessage.SetParameter(AuthConstants.TokenHintIdentifier, token);
            await Task.CompletedTask.ConfigureAwait(false);
        };
        options.SaveTokens = true;
    });

// Configure HttpClient for API calls
builder.Services.AddHttpClient("RecognitionCitizen", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["RecognitionApi:BaseUrl"]!);
});

// Register session management
builder.Services.AddSession(options =>
{
    options.Cookie.Name = CookieConstants.SessionCookieName;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.IdleTimeout = TimeSpan.FromHours(20);
    options.Cookie.IsEssential = true;
});

// Register essential services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRecognitionCitizenClient, RecognitionCitizenClient>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEligibilityService, EligibilityService>();
builder.Services.AddScoped<IFeatureFlagService, FeatureFlagService>();
builder.Services.AddScoped<IQuestionService, QuestionService>();

#endregion

var app = builder.Build();

#region Middleware

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

// Configure route mapping
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}"
);

#endregion

app.Run();