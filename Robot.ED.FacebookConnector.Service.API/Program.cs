using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.Validators;
using Robot.ED.FacebookConnector.Service.API.BackgroundServices;
using Robot.ED.FacebookConnector.Service.API.Middleware;
using Robot.ED.FacebookConnector.Service.API.Services;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/api-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Robot.ED.FacebookConnector.Service.API");

var builder = WebApplication.CreateBuilder(args);

// Add Serilog to the application
builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Robot.ED.FacebookConnector.Service.API", 
        Version = "v1",
        Description = "RPA Orchestrator API for Facebook Connector"
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
        b => b.MigrationsAssembly("Robot.ED.FacebookConnector.Service.API")));

// Configure settings
builder.Services.Configure<OrchestratorSettings>(
    builder.Configuration.GetSection("OrchestratorSettings"));

// Add FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<AllocateRequestValidator>();

// Register services
builder.Services.AddHttpClient<IWebhookService, WebhookService>();
builder.Services.AddScoped<IRpaAllocationService, RpaAllocationService>();
builder.Services.AddScoped<IRpaResultService, RpaResultService>();

// Add background services
builder.Services.AddHostedService<RpaAllocationBackgroundService>();
builder.Services.AddHostedService<DataExpirationBackgroundService>();

// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    var httpPort = builder.Configuration.GetValue<int?>("Kestrel:HttpPort") ?? 5000;
    var httpsPort = builder.Configuration.GetValue<int?>("Kestrel:HttpsPort") ?? 5001;
    
    serverOptions.ListenAnyIP(httpPort);
    serverOptions.ListenAnyIP(httpsPort, listenOptions =>
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
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Robot.ED.FacebookConnector.Service.API v1");
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
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
