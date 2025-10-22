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
    private readonly IWebhookService _webhookService;

    public RpaController(
        AppDbContext dbContext, 
        ILogger<RpaController> logger,
        IWebhookService webhookService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _webhookService = webhookService;
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

            // Find the queue item by TrackId
            var queueItem = await _dbContext.Queues
                .FirstOrDefaultAsync(q => q.TrackId == result.TrackId);

            if (queueItem == null)
            {
                _logger.LogWarning("Queue item not found for TrackId: {TrackId}", result.TrackId);
                return NotFound(new { error = "Queue item not found" });
            }

            // Create queue result
            var queueResult = new QueueResult
            {
                QueueId = queueItem.Id,
                ProcessedByRobotId = queueItem.AllocatedRobotId,
                HasError = result.HasError,
                ErrorMessage = result.ErrorMessage,
                TrackId = result.TrackId,
                Type = result.Type,
                MediaId = result.MediaId,
                Channel = result.Channel,
                Tag = result.Tag,
                ReceivedAt = DateTime.UtcNow
            };

            // Add messages
            foreach (var message in result.Messages ?? new List<string>())
            {
                queueResult.Messages.Add(new QueueResultMessage { Message = message });
            }

            // Add attachments
            foreach (var attachment in result.Attachments ?? new List<AttachmentDto>())
            {
                queueResult.Attachments.Add(new QueueResultAttachment
                {
                    AttachmentId = attachment.Id,
                    Name = attachment.Name,
                    ContentType = attachment.ContentType,
                    Url = attachment.Url
                });
            }

            _dbContext.QueueResults.Add(queueResult);

            // Update queue item
            queueItem.IsProcessed = true;
            queueItem.HasError = result.HasError;
            queueItem.ErrorMessage = result.ErrorMessage;
            queueItem.UpdatedAt = DateTime.UtcNow;

            // Mark robot as available again
            if (queueItem.AllocatedRobotId.HasValue)
            {
                var robot = await _dbContext.Robots.FindAsync(queueItem.AllocatedRobotId.Value);
                if (robot != null)
                {
                    robot.Available = true;
                    _logger.LogInformation("Robot {RobotId} marked as available", robot.Id);
                }
            }

            await _dbContext.SaveChangesAsync();

            // Forward result to webhook
            _ = Task.Run(async () =>
            {
                try
                {
                    await _webhookService.ForwardResultAsync(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error forwarding result to webhook");
                }
            });

            _logger.LogInformation("Result processed successfully for TrackId: {TrackId}", result.TrackId);

            return Ok(new { message = "Result processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing result");
            return StatusCode(500, new { error = "Internal server error" });
        }
        finally
        {
            // Memory cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
