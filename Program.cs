
using CustodialTaskList.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.DataProtection;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Load the supervisor password configuration
// from a separate file in Production.
if (builder.Environment.IsProduction())
{
    string passwordConfigPath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
        "CustodialTaskList",
        "supervisor.settings.json");

    builder.Configuration.AddJsonFile(
        passwordConfigPath,
        optional: false,
        reloadOnChange: false);
}


// Database configuration
var connectionString = builder.Configuration
    .GetConnectionString("TaskDatabase")
    ?? throw new InvalidOperationException(
        "TaskDatabase connection string is missing.");

var sqliteBuilder =
    new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder(
        connectionString);

// Make relative paths point to the application folder.
if (!Path.IsPathRooted(sqliteBuilder.DataSource))
{
    sqliteBuilder.DataSource = Path.Combine(
        builder.Environment.ContentRootPath,
        sqliteBuilder.DataSource);
}

builder.Services.AddDbContext<TaskDbContext>(options =>
    options.UseSqlite(sqliteBuilder.ConnectionString));


builder.Services.AddScoped<
    CustodialTaskList.Services.DailyTaskService>();

builder.Services.AddSingleton<
    CustodialTaskList.Security.SupervisorPasswordService>();

if (builder.Environment.IsProduction())
{
    string keyFolder = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.CommonApplicationData),
        "CustodialTaskList",
        "DataProtectionKeys");

    Directory.CreateDirectory(keyFolder);

    builder.Services.AddDataProtection()
        .SetApplicationName("CustodialTaskList")
        .PersistKeysToFileSystem(new DirectoryInfo(keyFolder))
        .ProtectKeysWithDpapi(protectToLocalMachine: true);
}


builder.Services
    .AddAuthentication(
        CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/Login";
        options.AccessDeniedPath = "/Admin/Login";

        // Give our application its own cookie name.
        options.Cookie.Name =
            "CustodialTaskList.Admin";

        // Prevent JavaScript from reading the cookie.
        options.Cookie.HttpOnly = true;

        // Only transmit authentication cookies
        // over secure HTTPS connections.
        options.Cookie.SecurePolicy =
            CookieSecurePolicy.Always;

        // Allow normal navigation while providing
        // protection against cross-site requests.
        options.Cookie.SameSite =
            SameSiteMode.Lax;

        // Require a new login after eight hours.
        options.ExpireTimeSpan =
            TimeSpan.FromHours(8);

        // Don't continually extend the session.
        options.SlidingExpiration = false;
    });


// Require login for every Admin page,
// except the login page itself.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
    options.Conventions.AllowAnonymousToPage(
        "/Admin/Login");
});

// Configure trusted forwarded headers for
// HTTPS reverse proxy connections.
builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto;

        // Only trust a reverse proxy running
        // on this same computer.
        options.KnownProxies.Add(
            IPAddress.Loopback);

        options.KnownProxies.Add(
            IPAddress.IPv6Loopback);

        options.ForwardLimit = 1;
    });

var app = builder.Build();

// Apply pending database migrations at startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider
        .GetRequiredService<TaskDbContext>();

    db.Database.Migrate();
}

// Process trusted proxy headers before
// HTTPS redirection or authentication.
app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication must come before authorization.
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
