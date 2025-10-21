using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.Validators;
using Robot.ED.FacebookConnector.Service.API.BackgroundServices;
using Robot.ED.FacebookConnector.Service.API.Middleware;
using Robot.ED.FacebookConnector.Service.API.Services;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddHttpClient<IRpaAllocationService, RpaAllocationService>();
builder.Services.AddScoped<IWebhookService, WebhookService>();
builder.Services.AddScoped<IRpaAllocationService, RpaAllocationService>();

// Add background services
builder.Services.AddHostedService<RpaAllocationBackgroundService>();
builder.Services.AddHostedService<DataExpirationBackgroundService>();

// Configure Kestrel for HTTPS
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000);
    serverOptions.ListenAnyIP(5001, listenOptions =>
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
