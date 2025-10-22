using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Common.DTOs;

namespace Robot.ED.FacebookConnector.Service.API.Services;

public class RpaAllocationService : IRpaAllocationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly HttpClient _httpClient;
    private readonly OrchestratorSettings _settings;
    private readonly ILogger<RpaAllocationService> _logger;

    public RpaAllocationService(
        IServiceScopeFactory scopeFactory,
        HttpClient httpClient,
        IOptions<OrchestratorSettings> settings,
        ILogger<RpaAllocationService> logger)
    {
        _scopeFactory = scopeFactory;
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task AllocateRpaAsync()
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        try
        {
            // Reset robots that have been occupied for too long (timeout)
            var timeoutThreshold = DateTime.UtcNow.AddMinutes(-_settings.RobotTimeoutMinutes);
            var timedOutRobots = await dbContext.Robots
                .Where(r => !r.Available && r.LastAllocatedAt < timeoutThreshold)
                .ToListAsync();

            foreach (var robot in timedOutRobots)
            {
                robot.Available = true;
                _logger.LogWarning("Robot {RobotId} timed out and marked as available", robot.Id);
            }

            if (timedOutRobots.Any())
            {
                await dbContext.SaveChangesAsync();
            }

            // Find available robot
            var availableRobot = await dbContext.Robots
                .FirstOrDefaultAsync(r => r.Available);

            if (availableRobot == null)
            {
                _logger.LogInformation("No available robots for allocation");
                return;
            }

            // Find pending queue item that hasn't exceeded retry limit
            var pendingQueue = await dbContext.Queues
                .Include(q => q.QueueTags)
                .Include(q => q.QueueData)
                .Where(q => !q.IsProcessed && q.RetryCount < _settings.MaxRetryAttempts)
                .OrderBy(q => q.CreatedAt)
                .FirstOrDefaultAsync();

            if (pendingQueue == null)
            {
                _logger.LogInformation("No pending queue items to process");
                return;
            }

            // Allocate robot to queue
            pendingQueue.AllocatedRobotId = availableRobot.Id;
            pendingQueue.RetryCount++;
            pendingQueue.UpdatedAt = DateTime.UtcNow;

            availableRobot.Available = false;
            availableRobot.LastAllocatedAt = DateTime.UtcNow;

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Allocated Robot {RobotId} to Queue {QueueId}, Attempt {RetryCount}", 
                availableRobot.Id, pendingQueue.Id, pendingQueue.RetryCount);

            // Send request to RPA asynchronously
            _ = Task.Run(async () =>
            {
                try
                {
                    await SendProcessRequestToRpaAsync(availableRobot.Url, pendingQueue);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending process request to RPA {RobotId}", availableRobot.Id);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RPA allocation");
        }
        finally
        {
            // Memory cleanup
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }

    private async Task SendProcessRequestToRpaAsync(string rpaUrl, Common.Models.Queue queue)
    {
        try
        {
            var processRequest = new ProcessRequestDto
            {
                QueueId = queue.Id,
                AiConfig = queue.AiConfig,
                TrackId = queue.TrackId,
                BridgeKey = queue.BridgeKey,
                OriginType = queue.OriginType,
                MediaId = queue.MediaId,
                Customer = queue.Customer,
                Channel = queue.Channel,
                Phrase = queue.Phrase,
                Tags = queue.QueueTags.Select(t => t.Tag).ToList(),
                Data = queue.QueueData.Select(d => new DataItemDto 
                { 
                    Header = d.Header, 
                    Value = d.Value 
                }).ToList()
            };

            var json = JsonSerializer.Serialize(processRequest);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{rpaUrl.TrimEnd('/')}/api/rpa/process";
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Process request sent successfully to RPA at {Url} for Queue {QueueId}", 
                    url, queue.Id);
            }
            else
            {
                _logger.LogWarning("Failed to send process request to RPA. Status: {StatusCode}, Queue: {QueueId}", 
                    response.StatusCode, queue.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending process request to RPA for Queue {QueueId}", queue.Id);
        }
    }
}
