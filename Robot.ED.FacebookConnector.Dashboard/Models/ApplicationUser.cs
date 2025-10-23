using Microsoft.AspNetCore.Identity;

namespace Robot.ED.FacebookConnector.Dashboard.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
