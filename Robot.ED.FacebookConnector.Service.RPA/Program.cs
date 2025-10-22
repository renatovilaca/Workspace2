using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Service.RPA.BackgroundServices;
using Robot.ED.FacebookConnector.Service.RPA.Middleware;
using Robot.ED.FacebookConnector.Service.RPA.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Robot.ED.FacebookConnector.Service.RPA",
        Version = "v1",
        Description = "RPA Service for Facebook Connector Automation"
    });
    c.AddSecurityDefinition("Bearer", new()
    {
        Description = "Authorization token",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Service.RPA")));

// Configure settings
builder.Services.Configure<RpaSettings>(
    builder.Configuration.GetSection("RpaSettings"));

// Register services
builder.Services.AddHttpClient<IRpaProcessingService, RpaProcessingService>();
builder.Services.AddScoped<IRpaProcessingService, RpaProcessingService>();

// Add background services
builder.Services.AddHostedService<DataExpirationBackgroundService>();

// Configure Kestrel
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(8080);
    serverOptions.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Robot.ED.FacebookConnector.Service.RPA v1");
    });
}

app.UseHttpsRedirection();

// Apply token authentication middleware to API routes
app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api/rpa"),
    appBuilder => appBuilder.UseMiddleware<TokenAuthenticationMiddleware>());

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        await dbContext.Database.MigrateAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Database migration failed. Make sure PostgreSQL is running and migrations are created.");
    }
}

app.Run();
