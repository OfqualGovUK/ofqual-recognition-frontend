using GovUk.Frontend.AspNetCore;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add in the GOVUK Kit
builder.Services.AddGovUkFrontend();

// Add in the controllers with views for our standard pages
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
}).AddMicrosoftIdentityUI();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    // This lambda determines whether user consent for non-essential cookies is needed for a given request.
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    // Handling SameSite cookie according to https://learn.microsoft.com/aspnet/core/security/samesite?view=aspnetcore-3.1
    options.HandleSameSiteCookieCompatibility();
});

// Configuration to sign-in users with Azure AD B2C
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options);
        options.Events ??= new OpenIdConnectEvents();
        options.Events.OnRedirectToIdentityProvider = async context =>
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
    });

builder.Services.AddRazorPages();

//Configuring appsettings section AzureAdB2C, into IOptions
builder.Services.AddOptions();
builder.Services.Configure<OpenIdConnectOptions>(builder.Configuration.GetSection("AzureAdB2C"));

// Matomo configuration
builder.Services.AddSingleton(_ =>
{
    var options = new MatomoModel();
    builder.Configuration.GetSection("Matomo").Bind(options);

    return options;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options => 
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});
builder.Services.AddScoped<IEligibilityService, EligibilityService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
// Add the Microsoft Identity Web cookie policy
app.UseCookiePolicy();
app.UseRouting();
// Add the ASP.NET Core authentication service
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    // Add endpoints for Razor pages
    endpoints.MapRazorPages();
});

app.Run();


