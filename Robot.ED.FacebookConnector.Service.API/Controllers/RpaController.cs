using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Common.Models;
using Robot.ED.FacebookConnector.Service.API.Services;

namespace Robot.ED.FacebookConnector.Service.API.Controllers;

[ApiController]
[Route("api/rpa")]
public class RpaController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RpaController> _logger;
    private readonly IRpaResultService _rpaResultService;

    public RpaController(
        AppDbContext dbContext, 
        ILogger<RpaController> logger,
        IRpaResultService rpaResultService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _rpaResultService = rpaResultService;
    }

    [HttpPost("allocate")]
    public async Task<IActionResult> Allocate([FromBody] AllocateRequestDto request)
    {
        try
        {
            _logger.LogInformation("Received allocation request for TrackId: {TrackId}", request.TrackId);

            var queue = new Queue
            {
                AiConfig = request.AiConfig,
                TrackId = request.TrackId,
                BridgeKey = request.BridgeKey,
                OriginType = request.OriginType,
                MediaId = request.MediaId,
                Customer = request.Customer,
                Channel = request.Channel,
                Phrase = request.Phrase,
                CreatedAt = DateTime.UtcNow,
                IsProcessed = false
            };

            // Add tags
            foreach (var tag in request.Tags ?? new List<string>())
            {
                queue.QueueTags.Add(new QueueTag { Tag = tag });
            }

            // Add data items
            foreach (var dataItem in request.Data ?? new List<DataItemDto>())
            {
                queue.QueueData.Add(new QueueData 
                { 
                    Header = dataItem.Header, 
                    Value = dataItem.Value 
                });
            }

            _dbContext.Queues.Add(queue);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Queue item created with Id: {Id}, UniqueId: {UniqueId}", queue.Id, queue.UniqueId);

            return StatusCode(201, new { id = queue.Id, uniqueId = queue.UniqueId });
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
