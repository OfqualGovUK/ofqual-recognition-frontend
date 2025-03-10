using GovUk.Frontend.AspNetCore;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Web.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGovUkFrontend();
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton(_ =>
{
    var options = new MatomoOptions();
    builder.Configuration.GetSection("Matomo").Bind(options);

    return options;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession();
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
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}"
);

app.Run();
