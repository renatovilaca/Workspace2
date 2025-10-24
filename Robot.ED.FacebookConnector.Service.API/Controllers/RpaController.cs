using Microsoft.AspNetCore.Mvc;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Service.API.Services;

namespace Robot.ED.FacebookConnector.Service.API.Controllers;

[ApiController]
[Route("api/rpa")]
public class RpaController : ControllerBase
{
    private readonly ILogger<RpaController> _logger;
    private readonly IRpaResultService _rpaResultService;
    private readonly IRpaAllocateService _rpaAllocateService;

    public RpaController(
        ILogger<RpaController> logger,
        IRpaResultService rpaResultService,
        IRpaAllocateService rpaAllocateService)
    {
        _logger = logger;
        _rpaResultService = rpaResultService;
        _rpaAllocateService = rpaAllocateService;
    }

    [HttpPost("allocate")]
    public async Task<IActionResult> Allocate([FromBody] AllocateRequestDto request)
    {
        try
        {
            var (queueId, uniqueId) = await _rpaAllocateService.AllocateAsync(request);
            return StatusCode(201, new { id = queueId, uniqueId = uniqueId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing allocation request");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            // Memory cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    [HttpPost("result")]
    public async Task<IActionResult> Result([FromBody] RpaResultDto result)
    {
        try
        {
            _logger.LogInformation("Received result for TrackId: {TrackId}", result.TrackId);

            var (success, errorMessage) = await _rpaResultService.ProcessResultAsync(result);

            if (!success)
            {
                if (errorMessage == "Queue item not found")
                {
                    return NotFound(new { error = errorMessage });
                }
                return StatusCode(500, new { error = errorMessage });
            }

            return Ok(new { message = "Result processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing result");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
