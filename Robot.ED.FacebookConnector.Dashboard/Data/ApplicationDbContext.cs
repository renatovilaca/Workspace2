using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;
using Robot.ED.FacebookConnector.Dashboard.Models;

namespace Robot.ED.FacebookConnector.Dashboard.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly AppDbContext _appDbContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        AppDbContext appDbContext) : base(options)
    {
        _appDbContext = appDbContext;
    }

    public DbSet<UserActionLog> UserActionLogs { get; set; } = null!;

    // Expose shared tables from AppDbContext
    public DbSet<Common.Models.Queue> Queues => _appDbContext.Queues;
    public DbSet<Common.Models.QueueResult> QueueResults => _appDbContext.QueueResults;
    public DbSet<Common.Models.Robot> Robots => _appDbContext.Robots;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<UserActionLog>(entity =>
        {
            entity.ToTable("user_action_log");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
        });
    }
}
