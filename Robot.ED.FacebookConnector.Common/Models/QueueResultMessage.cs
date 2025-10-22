using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class QueueResultMessage
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QueueResultId { get; set; }

    [ForeignKey(nameof(QueueResultId))]
    public QueueResult QueueResult { get; set; } = null!;

    public string Message { get; set; } = string.Empty;
}
