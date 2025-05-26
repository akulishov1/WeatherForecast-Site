using bb1.Services;
using bb1.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using bb1.Components.Models;
using static bb1.Services.WeatherDataRequestService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options => {
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});
builder.Services.AddMvc();

builder.Services.AddScoped<WeatherDataRequestService>();
builder.Services.AddScoped<WeatherDataRequestService.WeatherService>();
builder.Services.AddHttpClient<WeatherDataRequestService.WeatherService>();
builder.Services.AddScoped<WeatherRepositoryService>();
builder.Services.AddScoped<WeatherRecordsService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddHttpClient<IWeatherService, WeatherDataRequestService.WeatherService>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(options => {
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var DefaultConnectionStringToSQL = builder.Configuration.GetConnectionString("DbConnectionParameters");
builder.Services.AddDbContextFactory<WeatherDbContext>(options => options.UseSqlServer(DefaultConnectionStringToSQL));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseMigrationsEndPoint();
} else {
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

app.Run();
