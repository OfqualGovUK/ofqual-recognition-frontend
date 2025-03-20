using GovUk.Frontend.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Ofqual.Recognition.Frontend.Core.Models;
using Ofqual.Recognition.Frontend.Infrastructure.Services;
using Ofqual.Recognition.Frontend.Infrastructure.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddGovUkFrontend();
// Add services to the container.
builder.Services.AddControllersWithViews(options => 
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
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
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}"
);

app.Run();
