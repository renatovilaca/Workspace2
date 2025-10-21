using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Models;

namespace Robot.ED.FacebookConnector.Common.Configuration;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Queue> Queues { get; set; } = null!;
    public DbSet<QueueTag> QueueTags { get; set; } = null!;
    public DbSet<QueueData> QueueData { get; set; } = null!;
    public DbSet<QueueResult> QueueResults { get; set; } = null!;
    public DbSet<QueueResultMessage> QueueResultMessages { get; set; } = null!;
    public DbSet<QueueResultAttachment> QueueResultAttachments { get; set; } = null!;
    public DbSet<Models.Robot> Robots { get; set; } = null!;
    public DbSet<Token> Tokens { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Queue>(entity =>
        {
            entity.ToTable("queue");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UniqueId).IsRequired();
            entity.HasIndex(e => e.UniqueId).IsUnique();
            entity.HasIndex(e => e.TrackId);
            entity.HasIndex(e => e.IsProcessed);
            entity.HasIndex(e => e.CreatedAt);
        });

        modelBuilder.Entity<QueueTag>(entity =>
        {
            entity.ToTable("queue_tag");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.QueueTags)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QueueData>(entity =>
        {
            entity.ToTable("queue_data");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Queue)
                .WithMany(q => q.QueueData)
                .HasForeignKey(e => e.QueueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QueueResult>(entity =>
        {
            entity.ToTable("queue_result");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueueId);
            entity.HasIndex(e => e.TrackId);
            entity.HasIndex(e => e.ReceivedAt);
        });

        modelBuilder.Entity<QueueResultMessage>(entity =>
        {
            entity.ToTable("queue_result_message");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.QueueResult)
                .WithMany(q => q.Messages)
                .HasForeignKey(e => e.QueueResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<QueueResultAttachment>(entity =>
        {
            entity.ToTable("queue_result_attachment");
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.QueueResult)
                .WithMany(q => q.Attachments)
                .HasForeignKey(e => e.QueueResultId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Models.Robot>(entity =>
        {
            entity.ToTable("robot");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Available);
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.ToTable("token");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenValue).IsUnique();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("user");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TokenValue).IsUnique();
        });
    }
}
