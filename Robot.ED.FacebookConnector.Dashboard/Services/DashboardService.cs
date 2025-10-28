using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Dashboard.Data;
using Robot.ED.FacebookConnector.Dashboard.Models;
using Robot.ED.FacebookConnector.Dashboard.ViewModels;

namespace Robot.ED.FacebookConnector.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly AppDbContext _appDbContext;
    private readonly DashboardSettings _settings;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        ApplicationDbContext context,
        AppDbContext appDbContext,
        IOptions<DashboardSettings> settings,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _appDbContext = appDbContext;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(DateTime? filterDate = null)
    {
        try
        {
            var today = filterDate?.Date ?? DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            var viewModel = new DashboardViewModel
            {
                RefreshIntervalSeconds = _settings.RefreshIntervalSeconds
            };

            // Pending queue count
            viewModel.PendingQueueCount = await _appDbContext.Queues
                .CountAsync(q => !q.IsProcessed);

            // Processed today count
            viewModel.ProcessedTodayCount = await _appDbContext.Queues
                .CountAsync(q => q.IsProcessed && q.UpdatedAt >= today && q.UpdatedAt < tomorrow);

            // Error and success counts for today
            viewModel.ErrorCount = await _appDbContext.Queues
                .CountAsync(q => q.IsProcessed && q.HasError && q.UpdatedAt >= today && q.UpdatedAt < tomorrow);

            viewModel.SuccessCount = await _appDbContext.Queues
                .CountAsync(q => q.IsProcessed && !q.HasError && q.UpdatedAt >= today && q.UpdatedAt < tomorrow);

            // Robot availability
            viewModel.AvailableRobotsCount = await _appDbContext.Robots.CountAsync(r => r.Available);
            viewModel.OccupiedRobotsCount = await _appDbContext.Robots.CountAsync(r => !r.Available);

            // Most allocated robot today
            var mostAllocatedRobot = await _appDbContext.Queues
                .Where(q => q.AllocatedRobotId.HasValue && q.CreatedAt >= today && q.CreatedAt < tomorrow)
                .GroupBy(q => q.AllocatedRobotId)
                .Select(g => new { RobotId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();

            if (mostAllocatedRobot != null)
            {
                viewModel.MostAllocatedRobotId = mostAllocatedRobot.RobotId;
                var robot = await _appDbContext.Robots
                    .FirstOrDefaultAsync(r => r.Id == mostAllocatedRobot.RobotId);
                viewModel.MostAllocatedRobotName = robot?.Name;
            }

            // Last processed time
            var lastProcessed = await _appDbContext.Queues
                .Where(q => q.IsProcessed && q.UpdatedAt.HasValue)
                .OrderByDescending(q => q.UpdatedAt)
                .FirstOrDefaultAsync();

            viewModel.LastProcessedTime = lastProcessed?.UpdatedAt;

            // Last request received time
            var lastRequest = await _appDbContext.Queues
                .OrderByDescending(q => q.CreatedAt)
                .FirstOrDefaultAsync();

            viewModel.LastRequestReceivedTime = lastRequest?.CreatedAt;

            // Check for recent errors (last successful processing clears the banner)
            var lastError = await _appDbContext.QueueResults
                .Where(qr => qr.HasError)
                .OrderByDescending(qr => qr.ReceivedAt)
                .FirstOrDefaultAsync();

            var lastSuccess = await _appDbContext.QueueResults
                .Where(qr => !qr.HasError)
                .OrderByDescending(qr => qr.ReceivedAt)
                .FirstOrDefaultAsync();

            if (lastError != null && (lastSuccess == null || lastError.ReceivedAt > lastSuccess.ReceivedAt))
            {
                viewModel.HasRecentError = true;
                viewModel.LastErrorMessage = lastError.ErrorMessage;
                viewModel.LastErrorTrackId = lastError.TrackId;
                viewModel.LastErrorTime = lastError.ReceivedAt;
            }

            // Recent queues (last 10)
            viewModel.RecentQueues = await _appDbContext.Queues
                .OrderByDescending(q => q.CreatedAt)
                .Take(10)
                .Select(q => new QueueStatusItem
                {
                    Id = q.Id,
                    TrackId = q.TrackId,
                    Channel = q.Channel,
                    CreatedAt = q.CreatedAt,
                    IsProcessed = q.IsProcessed,
                    HasError = q.HasError,
                    ErrorMessage = q.ErrorMessage
                })
                .ToListAsync();

            // Robot statuses
            var robots = await _appDbContext.Robots.ToListAsync();
            foreach (var robot in robots)
            {
                var processedToday = await _appDbContext.Queues
                    .CountAsync(q => q.AllocatedRobotId == robot.Id && 
                                   q.CreatedAt >= today && q.CreatedAt < tomorrow);

                viewModel.RobotStatuses.Add(new RobotStatusItem
                {
                    Id = robot.Id,
                    Name = robot.Name,
                    Available = robot.Available,
                    ProcessedToday = processedToday,
                    LastAllocatedAt = robot.LastAllocatedAt
                });
            }

            return viewModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard data");
            throw;
        }
    }

    public async Task LogUserActionAsync(string userId, string userName, string action, string? details = null)
    {
        try
        {
            var log = new UserActionLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.UserActionLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging user action");
        }
    }
}
