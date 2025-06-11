using bb1.Services;
using bb1.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using bb1.Components.Models;
using bb1.Services.WeatherInterfaces;
using bb1.Services.WDRequestListFormats;
using System.Reflection;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);


builder.WebHost.ConfigureKestrel(options =>
{
    builder.Configuration.GetSection("Kestrel").Bind(options);
});

var parserInterface = typeof(IWeatherDataParser);
var parserTypes = Assembly.GetExecutingAssembly()
    .GetTypes()
    .Where(t => parserInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

foreach (var parserType in parserTypes)
{
    builder.Services.AddScoped(typeof(IWeatherDataParser), parserType);
}

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddDevExpressBlazor(options =>
{
    options.BootstrapVersion = DevExpress.Blazor.BootstrapVersion.v5;
    options.SizeMode = DevExpress.Blazor.SizeMode.Medium;
});

builder.Services.AddMvc();
builder.Services.AddHttpClient();

builder.Services.AddScoped<WeatherDataRequestService>();
builder.Services.AddScoped<WeatherRecordsService>();
builder.Services.AddScoped<IWeatherRecords, WeatherRecordsService>();
builder.Services.AddScoped<WeatherProcessorService>();
builder.Services.AddScoped<IWeatherDataProcessor, WeatherProcessorService>();
builder.Services.AddScoped<IWeatherDRService, WeatherDataRequestService>();
builder.Services.AddScoped<WeatherDataVerification>();

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = IdentityConstants.ApplicationScheme;
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
.AddIdentityCookies();

var DefaultConnectionStringToSQL = builder.Configuration.GetConnectionString("DbConnectionParameters");
builder.Services.AddDbContextFactory<WeatherDbContext>(options =>
    options.UseSqlServer(DefaultConnectionStringToSQL));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AllowAnonymous();

var runTask = app.RunAsync();
var isRunningInIDE = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_IDE") == "true";

if (!isRunningInIDE) 
{
    try
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "http://localhost:5000/",
            UseShellExecute = true
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Couldn't launch the server: {ex.Message}");
    }
}

await runTask;