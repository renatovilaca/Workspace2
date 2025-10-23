using System.ComponentModel.DataAnnotations;

namespace Robot.ED.FacebookConnector.Dashboard.Models;

public class UserActionLog
{
    [Key]
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public string? Details { get; set; }
}
