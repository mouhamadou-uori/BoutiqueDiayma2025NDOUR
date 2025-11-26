using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using P2FixAnAppDotNetCode.Models;
using P2FixAnAppDotNetCode.Models.Repositories;
using P2FixAnAppDotNetCode.Models.Services;
using Microsoft.Extensions.FileProviders;
using System.IO;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

var builder = WebApplication.CreateBuilder(args);

// --- Smart URL binding: choose a free port if none provided ---
// Priority order for URLs:
// 1) Command-line: --urls
// 2) Environment: ASPNETCORE_URLS
// 3) Auto-pick: prefer 5000, then 5001, else an ephemeral free port
bool UrlProvidedViaArgs = args.Any(a => a.Equals("--urls", StringComparison.OrdinalIgnoreCase) || a.StartsWith("--urls=", StringComparison.OrdinalIgnoreCase));
bool UrlProvidedViaEnv = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS"));

if (!UrlProvidedViaArgs && !UrlProvidedViaEnv)
{
    int PickFreePort(params int[] preferred)
    {
        // Try preferred ports first; if busy, fall back to an ephemeral port
        foreach (var p in preferred)
        {
            if (IsPortFree(p)) return p;
        }
        // Ephemeral: ask OS for a free port by binding to 0
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        int port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    bool IsPortFree(int port)
    {
        try
        {
            var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch
        {
            return false;
        }
    }

    var chosenPort = PickFreePort(5000, 5001);
    var chosenUrl = $"http://127.0.0.1:{chosenPort}";
    builder.WebHost.UseUrls(chosenUrl);
}

// Services (ConfigureServices migrated from Startup)
builder.Services.AddLocalization(opts => { opts.ResourcesPath = "Resources"; });
builder.Services.AddSingleton<ICart, Cart>();
builder.Services.AddSingleton<ILanguageService, LanguageService>();
builder.Services.AddTransient<IProductService, ProductService>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<IOrderRepository, OrderRepository>();

builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache(); // required for Session in modern ASP.NET Core
builder.Services.AddSession();

builder.Services
    .AddControllersWithViews()
    .AddViewLocalization(
        LanguageViewLocationExpanderFormat.Suffix,
        opts => { opts.ResourcesPath = "Resources"; })
    .AddDataAnnotationsLocalization();

builder.Services.Configure<RequestLocalizationOptions>(opts =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en-GB"),
        new CultureInfo("en-US"),
        new CultureInfo("en"),
        new CultureInfo("fr-FR"),
        new CultureInfo("fr"),
    };

    opts.DefaultRequestCulture = new RequestCulture("en");
    // Formatting numbers, dates, etc.
    opts.SupportedCultures = supportedCultures;
    // UI strings that we have localized.
    opts.SupportedUICultures = supportedCultures;
});

var app = builder.Build();

// Middleware pipeline (Configure migrated from Startup)
// Serve static files when a wwwroot exists either next to the built executable (AppContext.BaseDirectory)
// or in the content root (useful when running via dotnet run).
var publishWebRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot");
var projectWebRoot = Path.Combine(app.Environment.ContentRootPath, "wwwroot");

if (Directory.Exists(publishWebRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(publishWebRoot)
    });
}
else if (Directory.Exists(projectWebRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(projectWebRoot)
    });
}
// If neither exists, skip UseStaticFiles to avoid noisy warnings.

var locOptions = app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
app.UseRequestLocalization(locOptions);

app.UseRouting();
app.UseSession();

// Print a small startup banner so running the EXE shows what to do next.
try
{
    Console.WriteLine("[INFO] Diayma web server starting...");
    var urls = app.Urls.Any() ? string.Join(", ", app.Urls) : "(auto)";
    Console.WriteLine("[INFO] Listening on: " + urls);
    Console.WriteLine("[INFO] Open your browser to: " + (app.Urls.FirstOrDefault() ?? "http://127.0.0.1:5000"));
    Console.WriteLine("[INFO] Press Ctrl+C to shut down.");
}
catch { /* ignore console write failures in restricted environments */ }

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Product}/{action=Index}/{id?}");

app.Run();
