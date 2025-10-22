using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Service.RPA.Controllers;

[ApiController]
[Route("api")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext dbContext, ILogger<HealthController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    [HttpGet("health")]
    public async Task<IActionResult> Health()
    {
        try
        {
            // Check database connectivity
            await _dbContext.Database.CanConnectAsync();

            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                service = "Robot.ED.FacebookConnector.Service.RPA"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                service = "Robot.ED.FacebookConnector.Service.RPA",
                error = ex.Message
            });
        }
    }
}
