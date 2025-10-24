using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.API.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value?.ToLower() ?? string.Empty;

        // Skip authentication for health check and swagger
        if (path.Contains("/health") || path.Contains("/swagger") || path.Contains("/api-docs"))
        {
            await _next(context);
            return;
        }

        // Check for token in header
        if (!context.Request.Headers.TryGetValue("Authorization", out var tokenHeaderValue))
        {
            _logger.LogWarning("Missing Authorization header");
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Missing token" });
            return;
        }

        var token = tokenHeaderValue.ToString().Replace("Bearer ", "").Trim();

        // Validate token in database
        var tokenExists = await dbContext.Tokens.AnyAsync(t => t.TokenValue == token);

        if (!tokenExists)
        {
            _logger.LogWarning("Invalid token attempted: {Token}", token.Substring(0, Math.Min(10, token.Length)));
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized: Invalid token" });
            return;
        }

        await _next(context);
    }
}
