using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Robot.ED.FacebookConnector.Common.Models;

public class Robot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Url { get; set; } = string.Empty;

    public bool Available { get; set; } = true;

    public DateTime? LastAllocatedAt { get; set; }

    public string? Token { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
