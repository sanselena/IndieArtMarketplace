using IndieArtMarketplace.Business.Services;
using IndieArtMarketplace.DAL;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Http.Features;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using ImageKit;
using IndieArtMarketplace.Models;

// Adding a dummy comment to force a new deploy on Render.com

var builder = WebApplication.CreateBuilder(args);

// Configure ImageKit settings
builder.Services.Configure<ImageKitSettings>(builder.Configuration.GetSection("ImageKit"));
builder.Services.AddSingleton(sp =>
{
    var imageKitSettings = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ImageKitSettings>>().Value;
    return new ImageKitClient(imageKitSettings.ImageKitPublicKey, imageKitSettings.ImageKitPrivateKey, imageKitSettings.ImageKitUrlEndpoint);
});

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure file upload limits
builder.Services.Configure<IISServerOptions>(options =>
{
    options.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 10 * 1024 * 1024; // 10MB
});

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024; // 10MB
});

// Configure Data Protection to use PostgreSQL
var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "DataProtection-Keys");
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<AppDbContext>()
    .SetApplicationName("IndieArtMarketplace");

// Add DbContext with connection validation
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
        npgsqlOptions.CommandTimeout(30);
    });
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    options.EnableSensitiveDataLogging(false);
    options.EnableDetailedErrors(false);
});

// Register Services
builder.Services.AddScoped<UserService>();

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add authentication configuration
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "Cookies";
})
.AddCookie("Cookies", options =>
{
    options.LoginPath = "/User/Login";
    options.LogoutPath = "/User/Logout";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Add logging for WebRootPath and uploads directory
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("WebRootPath: {WebRootPath}", builder.Environment.WebRootPath);

var uploadsPath = Path.Combine(builder.Environment.WebRootPath, "uploads");
logger.LogInformation("Expected uploads directory path: {UploadsPath}", uploadsPath);
logger.LogInformation("Does uploads directory exist? {Exists}", Directory.Exists(uploadsPath));

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // This serves files from wwwroot

// Explicitly serve files from the 'uploads' directory within wwwroot
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.WebRootPath, "uploads")),
    RequestPath = "/uploads"
});

app.UseRouting();

app.UseSession();

// Add authentication configuration
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Validate database connection
try
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    if (!context.Database.CanConnect())
    {
        throw new Exception("Cannot connect to database. Please check your connection string and PostgreSQL status.");
    }
    Console.WriteLine("Database connection successful!");
}
catch (Exception ex)
{
    Console.WriteLine($"Database operation failed during startup: {ex.Message}");
    // Depending on severity, you might want to re-throw in production
    // throw; // Uncomment to halt startup on migration failure
}

app.Run();
