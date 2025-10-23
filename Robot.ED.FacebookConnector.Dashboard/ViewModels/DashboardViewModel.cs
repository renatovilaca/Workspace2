namespace Robot.ED.FacebookConnector.Dashboard.ViewModels;

public class DashboardViewModel
{
    public int PendingQueueCount { get; set; }
    public int ProcessedTodayCount { get; set; }
    public int ErrorCount { get; set; }
    public int SuccessCount { get; set; }
    public int AvailableRobotsCount { get; set; }
    public int OccupiedRobotsCount { get; set; }
    public int? MostAllocatedRobotId { get; set; }
    public string? MostAllocatedRobotName { get; set; }
    public DateTime? LastProcessedTime { get; set; }
    public DateTime? LastRequestReceivedTime { get; set; }
    public bool HasRecentError { get; set; }
    public string? LastErrorMessage { get; set; }
    public string? LastErrorTrackId { get; set; }
    public DateTime? LastErrorTime { get; set; }
    public int RefreshIntervalSeconds { get; set; } = 30;
    public List<QueueStatusItem> RecentQueues { get; set; } = new();
    public List<RobotStatusItem> RobotStatuses { get; set; } = new();
}

public class QueueStatusItem
{
    public int Id { get; set; }
    public string TrackId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsProcessed { get; set; }
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
}

public class RobotStatusItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Available { get; set; }
    public int ProcessedToday { get; set; }
    public DateTime? LastAllocatedAt { get; set; }
}
