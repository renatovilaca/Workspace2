using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class QueueResultAttachment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int QueueResultId { get; set; }

    [ForeignKey(nameof(QueueResultId))]
    public QueueResult QueueResult { get; set; } = null!;

    public string AttachmentId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;
}
