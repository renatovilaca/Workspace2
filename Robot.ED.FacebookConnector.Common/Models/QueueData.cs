using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class QueueData
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QueueId { get; set; }

    [ForeignKey(nameof(QueueId))]
    public Queue Queue { get; set; } = null!;

    public string Header { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}
