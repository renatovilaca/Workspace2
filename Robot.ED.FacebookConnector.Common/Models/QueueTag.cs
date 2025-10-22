using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class QueueTag
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QueueId { get; set; }

    [ForeignKey(nameof(QueueId))]
    public Queue Queue { get; set; } = null!;

    public string Tag { get; set; } = string.Empty;
}
