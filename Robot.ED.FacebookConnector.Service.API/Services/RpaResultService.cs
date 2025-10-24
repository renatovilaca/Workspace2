using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;
using Robot.ED.FacebookConnector.Common.Models;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public class RpaResultService : IRpaResultService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<RpaResultService> _logger;
    private readonly IWebhookService _webhookService;

    public RpaResultService(
        AppDbContext dbContext,
        ILogger<RpaResultService> logger,
        IWebhookService webhookService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _webhookService = webhookService;
    }

    public async Task<(bool Success, string? ErrorMessage)> ProcessResultAsync(RpaResultDto result)
    {
        try
        {
            _logger.LogInformation("Processing result for TrackId: {TrackId}", result.TrackId);

            // Find the queue item by TrackId
            var queueItem = await _dbContext.Queues
                .FirstOrDefaultAsync(q => q.TrackId == result.TrackId);

            if (queueItem == null)
            {
                _logger.LogWarning("Queue item not found for TrackId: {TrackId}", result.TrackId);
                return (false, "Queue item not found");
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
                    _dbContext.Robots.Update(robot);
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

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing result for TrackId: {TrackId}", result.TrackId);
            return (false, "Internal server error");
        }
        finally
        {
            // Memory cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
