using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Infrastructure.Persistence.Entities;

namespace MundialCorporativo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<DomainEventLog> DomainEventLogs => Set<DomainEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(builder =>
        {
            builder.ToTable("Teams");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Player>(builder =>
        {
            builder.ToTable("Players");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.HasOne<Team>()
                .WithMany()
                .HasForeignKey(x => x.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Match>(builder =>
        {
            builder.ToTable("Matches");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasConversion<int>().IsRequired();
            builder.HasOne<Team>().WithMany().HasForeignKey(x => x.HomeTeamId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Team>().WithMany().HasForeignKey(x => x.AwayTeamId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<IdempotencyRecord>(builder =>
        {
            builder.ToTable("IdempotencyRecords");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Key).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Path).HasMaxLength(400).IsRequired();
            builder.Property(x => x.Method).HasMaxLength(20).IsRequired();
            builder.HasIndex(x => new { x.Key, x.Path, x.Method }).IsUnique();
        });

        modelBuilder.Entity<DomainEventLog>(builder =>
        {
            builder.ToTable("DomainEventLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventType).HasMaxLength(300).IsRequired();
            builder.Property(x => x.TraceId).HasMaxLength(200).IsRequired();
        });
    }
}
