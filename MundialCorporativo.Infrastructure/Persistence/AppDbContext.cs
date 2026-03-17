using Microsoft.EntityFrameworkCore;
using MundialCorporativo.Domain.Common;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Domain.Enums;
using MundialCorporativo.Infrastructure.Persistence.Entities;

namespace MundialCorporativo.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private static readonly Guid Team1Id = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Team2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Team3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private static readonly Guid Team4Id = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static readonly DateTime SeedBaseDateUtc = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<MatchScore> MatchScores => Set<MatchScore>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<DomainEventLog> DomainEventLogs => Set<DomainEventLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>(builder =>
        {
            builder.ToTable("Teams");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Ignore(x => x.DomainEvents);
            builder.Ignore(x => x.Players);
        });

        modelBuilder.Entity<Player>(builder =>
        {
            builder.ToTable("Players");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.GoalsAgainst).HasDefaultValue(0).IsRequired();
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
            builder.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Entity<MatchScore>(builder =>
        {
            builder.ToTable("MatchScores");
            builder.HasKey(x => x.Id);
            builder.HasOne<Match>().WithMany().HasForeignKey(x => x.MatchId).OnDelete(DeleteBehavior.Restrict);
            builder.HasOne<Player>().WithMany().HasForeignKey(x => x.PlayerId).OnDelete(DeleteBehavior.Restrict);
            builder.Ignore(x => x.DomainEvents);
        });

        modelBuilder.Ignore<DomainEvent>();

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

        ConfigureSeedData(modelBuilder);
    }

    private static void ConfigureSeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>().HasData(
            new { Id = Team1Id, Name = "Tigres Tech" },
            new { Id = Team2Id, Name = "Leones Data" },
            new { Id = Team3Id, Name = "Halcones DevOps" },
            new { Id = Team4Id, Name = "Toros Cloud" });

        var players = new List<object>();
        var playerIds = new List<Guid>();
        var playerCounter = 1;
        var teams = new[]
        {
            new { TeamId = Team1Id, Prefix = "Tigres Tech" },
            new { TeamId = Team2Id, Prefix = "Leones Data" },
            new { TeamId = Team3Id, Prefix = "Halcones DevOps" },
            new { TeamId = Team4Id, Prefix = "Toros Cloud" }
        };

        foreach (var team in teams)
        {
            for (var i = 1; i <= 5; i++)
            {
                var playerId = Guid.Parse($"55555555-5555-5555-5555-{playerCounter:000000000000}");
                playerIds.Add(playerId);
                players.Add(new
                {
                    Id = playerId,
                    TeamId = team.TeamId,
                    FullName = $"{team.Prefix} Jugador {i}",
                    JerseyNumber = i,
                    GoalsScored = 0,
                    GoalsAgainst = 0
                });
                playerCounter++;
            }
        }

        players[0] = new
        {
            Id = playerIds[0],
            TeamId = Team1Id,
            FullName = "Tigres Tech Jugador 1",
            JerseyNumber = 1,
            GoalsScored = 2,
            GoalsAgainst = 0
        };
        players[1] = new
        {
            Id = playerIds[1],
            TeamId = Team1Id,
            FullName = "Tigres Tech Jugador 2",
            JerseyNumber = 2,
            GoalsScored = 1,
            GoalsAgainst = 0
        };
        players[2] = new
        {
            Id = playerIds[2],
            TeamId = Team1Id,
            FullName = "Tigres Tech Jugador 3",
            JerseyNumber = 3,
            GoalsScored = 1,
            GoalsAgainst = 0
        };
        players[3] = new
        {
            Id = playerIds[3],
            TeamId = Team1Id,
            FullName = "Tigres Tech Jugador 4",
            JerseyNumber = 4,
            GoalsScored = 1,
            GoalsAgainst = 0
        };
        players[4] = new
        {
            Id = playerIds[4],
            TeamId = Team1Id,
            FullName = "Tigres Tech Jugador 5",
            JerseyNumber = 5,
            GoalsScored = 1,
            GoalsAgainst = 0
        };
        players[5] = new
        {
            Id = playerIds[5],
            TeamId = Team2Id,
            FullName = "Leones Data Jugador 1",
            JerseyNumber = 1,
            GoalsScored = 1,
            GoalsAgainst = 0
        };

        modelBuilder.Entity<Player>().HasData(players.ToArray());

        modelBuilder.Entity<Match>().HasData(
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa1"),
                HomeTeamId = Team1Id,
                AwayTeamId = Team2Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(-6),
                Status = MatchStatus.Completed,
                HomeScore = 2,
                AwayScore = 1
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa2"),
                HomeTeamId = Team3Id,
                AwayTeamId = Team4Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(-5),
                Status = MatchStatus.Completed,
                HomeScore = 0,
                AwayScore = 0
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa3"),
                HomeTeamId = Team1Id,
                AwayTeamId = Team3Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(-3),
                Status = MatchStatus.Completed,
                HomeScore = 1,
                AwayScore = 3
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa4"),
                HomeTeamId = Team2Id,
                AwayTeamId = Team4Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(1),
                Status = MatchStatus.Scheduled,
                HomeScore = (int?)null,
                AwayScore = (int?)null
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa5"),
                HomeTeamId = Team1Id,
                AwayTeamId = Team4Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(3),
                Status = MatchStatus.Scheduled,
                HomeScore = (int?)null,
                AwayScore = (int?)null
            },
            new
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaa6"),
                HomeTeamId = Team2Id,
                AwayTeamId = Team3Id,
                MatchDateUtc = SeedBaseDateUtc.AddDays(5),
                Status = MatchStatus.Scheduled,
                HomeScore = (int?)null,
                AwayScore = (int?)null
            });
    }
}
