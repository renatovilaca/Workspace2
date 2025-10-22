using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class Queue
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public Guid UniqueId { get; set; } = Guid.NewGuid();

    public string AiConfig { get; set; } = string.Empty;

    public string TrackId { get; set; } = string.Empty;

    public string BridgeKey { get; set; } = string.Empty;

    public string OriginType { get; set; } = string.Empty;

    public string MediaId { get; set; } = string.Empty;

    public string Customer { get; set; } = string.Empty;

    public string Channel { get; set; } = string.Empty;

    public string Phrase { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public int? AllocatedRobotId { get; set; }

    [ForeignKey(nameof(AllocatedRobotId))]
    public Robot? AllocatedRobot { get; set; }

    public int RetryCount { get; set; } = 0;

    public bool IsProcessed { get; set; } = false;

    public bool HasError { get; set; } = false;

    public string? ErrorMessage { get; set; }

    public ICollection<QueueTag> QueueTags { get; set; } = new List<QueueTag>();

    public ICollection<QueueData> QueueData { get; set; } = new List<QueueData>();
}
