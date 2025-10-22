using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Dashboard.Data;
using Robot.ED.FacebookConnector.Dashboard.Models;
using Robot.ED.FacebookConnector.Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure DbContexts
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Dashboard")));

// Configure Identity
builder.Services.AddDefaultIdentity<ApplicationUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configure settings
builder.Services.Configure<DashboardSettings>(
    builder.Configuration.GetSection("DashboardSettings"));

// Register services
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(7000);
    serverOptions.ListenAnyIP(7001, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Starting database initialization...");
        
        // Apply migrations for shared AppDbContext
        var appDbContext = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Applying AppDbContext migrations...");
        await appDbContext.Database.MigrateAsync();
        logger.LogInformation("AppDbContext migrations applied successfully");
        
        // Apply migrations for ApplicationDbContext (Identity)
        var context = services.GetRequiredService<ApplicationDbContext>();
        logger.LogInformation("Applying ApplicationDbContext migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("ApplicationDbContext migrations applied successfully");

        // Seed roles and admin user
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        // Create roles
        logger.LogInformation("Creating roles...");
        string[] roleNames = { "Admin", "User" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Role '{RoleName}' created", roleName);
            }
            else
            {
                logger.LogInformation("Role '{RoleName}' already exists", roleName);
            }
        }

        // Create admin user
        logger.LogInformation("Creating admin user...");
        var adminEmail = "admin@localhost.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin@123");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
                logger.LogInformation("Admin user created successfully with email: {Email}", adminEmail);
            }
            else
            {
                logger.LogError("Failed to create admin user. Errors: {Errors}", 
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists");
        }

        // Seed sample data if database is empty
        if (!await appDbContext.Robots.AnyAsync())
        {
            appDbContext.Robots.AddRange(
                new Robot.ED.FacebookConnector.Common.Models.Robot { Name = "RPA 1", Url = "http://localhost:8080", Available = true },
                new Robot.ED.FacebookConnector.Common.Models.Robot { Name = "RPA 2", Url = "http://localhost:8082", Available = true }
            );
            await appDbContext.SaveChangesAsync();
        }

        if (!await appDbContext.Tokens.AnyAsync())
        {
            appDbContext.Tokens.Add(new Robot.ED.FacebookConnector.Common.Models.Token
            {
                UserName = "api-client",
                TokenValue = "default-token",
                Created = DateTime.UtcNow
            });
            await appDbContext.SaveChangesAsync();
        }

        if (!await appDbContext.Users.AnyAsync())
        {
            appDbContext.Users.Add(new Robot.ED.FacebookConnector.Common.Models.User
            {
                UserName = "rpa-service",
                TokenValue = "default-token",
                Created = DateTime.UtcNow
            });
            await appDbContext.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}

app.Run();
