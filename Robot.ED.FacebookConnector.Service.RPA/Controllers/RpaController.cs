using Microsoft.AspNetCore.Mvc;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Service.RPA.Services;

namespace Robot.ED.FacebookConnector.Service.RPA.Controllers;

[ApiController]
[Route("api/rpa")]
public class RpaController : ControllerBase
{
    private readonly ILogger<RpaController> _logger;
    private readonly IRpaProcessingService _processingService;

    public RpaController(
        ILogger<RpaController> logger,
        IRpaProcessingService processingService)
    {
        _logger = logger;
        _processingService = processingService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] ProcessRequestDto request)
    {
        try
        {
            _logger.LogInformation("Received process request for QueueId: {QueueId}, TrackId: {TrackId}", 
                request.QueueId, request.TrackId);

            // Process asynchronously - don't wait for completion
            _ = Task.Run(async () =>
            {
                try
                {
                    await _processingService.ProcessAsync(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queue {QueueId}", request.QueueId);
                }
            });

            return Accepted(new { message = "Request accepted for processing", queueId = request.QueueId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting process request");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
