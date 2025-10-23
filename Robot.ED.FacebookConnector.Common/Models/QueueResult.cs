using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class QueueResult
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QueueId { get; set; }

    [ForeignKey(nameof(QueueId))]
    public Queue Queue { get; set; } = null!;

    public int? ProcessedByRobotId { get; set; }

    [ForeignKey(nameof(ProcessedByRobotId))]
    public Robot? ProcessedByRobot { get; set; }

    public bool HasError { get; set; }

    public string? ErrorMessage { get; set; }

    public string TrackId { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string MediaId { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string? Tag { get; set; }

    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    public ICollection<QueueResultMessage> Messages { get; set; } = new List<QueueResultMessage>();

    public ICollection<QueueResultAttachment> Attachments { get; set; } = new List<QueueResultAttachment>();
}
