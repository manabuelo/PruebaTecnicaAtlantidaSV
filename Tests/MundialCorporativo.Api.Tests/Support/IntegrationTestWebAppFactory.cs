using System.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Application.Common;
using MundialCorporativo.Application.DTOs;
using MundialCorporativo.Application.Matches;
using MundialCorporativo.Application.Players;
using MundialCorporativo.Application.Standings;
using MundialCorporativo.Application.Teams;
using MundialCorporativo.Domain.Entities;
using MundialCorporativo.Infrastructure.Persistence;

namespace MundialCorporativo.Api.Tests.Support;

/// <summary>
/// Spins up the real ASP.NET Core pipeline against an InMemory EF Core database.
/// Each factory instance uses a uniquely-named database so parallel test classes are isolated.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString("N");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Tell the app it is in Testing mode (no real DB connection attempted)
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // ── Replace AppDbContext with InMemory ──────────────────────────
            var existing = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>))
                .ToList();
            foreach (var d in existing) services.Remove(d);

            services.AddDbContext<AppDbContext>(opt =>
                opt.UseInMemoryDatabase(_dbName));

            // ── Replace UnitOfWork (InMemory doesn't support real transactions) ──
            services.AddScoped<IUnitOfWork, FakeInMemoryUnitOfWork>();

            // ── Replace Dapper read-connection factory (won't be reached) ──
            services.AddScoped<IReadDbConnectionFactory, NullReadDbConnectionFactory>();

            // ── Replace Dapper query handlers with InMemory-backed equivalents ──
            services.AddScoped<IQueryHandler<GetTeamByIdQuery, TeamDto?>, InMemoryTeamQueryHandlers>();
            services.AddScoped<IQueryHandler<ListTeamsQuery, PagedResult<TeamDto>>, InMemoryTeamQueryHandlers>();

            services.AddScoped<IQueryHandler<GetPlayerByIdQuery, PlayerDto?>, InMemoryPlayerQueryHandlers>();
            services.AddScoped<IQueryHandler<ListPlayersQuery, PagedResult<PlayerDto>>, InMemoryPlayerQueryHandlers>();

            services.AddScoped<IQueryHandler<GetMatchByIdQuery, MatchDto?>, InMemoryMatchQueryHandlers>();
            services.AddScoped<IQueryHandler<ListMatchesQuery, PagedResult<MatchDto>>, InMemoryMatchQueryHandlers>();

            services.AddScoped<IQueryHandler<GetStandingsQuery, PagedResult<StandingDto>>, InMemoryStandingQueryHandlers>();
            services.AddScoped<IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerDto>>, InMemoryStandingQueryHandlers>();
        });
    }
}

// ── Fake infrastructure ──────────────────────────────────────────────────────

internal sealed class FakeInMemoryUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public FakeInMemoryUnitOfWork(AppDbContext db) => _db = db;
    public Task<int> CommitAsync(CancellationToken cancellationToken) =>
        _db.SaveChangesAsync(cancellationToken);
}

internal sealed class NullReadDbConnectionFactory : IReadDbConnectionFactory
{
    public Task<IDbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken) =>
        throw new NotSupportedException("Dapper connections unavailable in tests; use InMemory query handlers.");
}

// ── InMemory query handlers ──────────────────────────────────────────────────

internal sealed class InMemoryTeamQueryHandlers :
    IQueryHandler<GetTeamByIdQuery, TeamDto?>,
    IQueryHandler<ListTeamsQuery, PagedResult<TeamDto>>
{
    private readonly AppDbContext _db;
    public InMemoryTeamQueryHandlers(AppDbContext db) => _db = db;

    public async Task<TeamDto?> Handle(GetTeamByIdQuery query, CancellationToken ct)
    {
        var team = await _db.Teams.FindAsync(new object[] { query.TeamId }, ct);
        return team is null ? null : new TeamDto(team.Id, team.Name);
    }

    public Task<PagedResult<TeamDto>> Handle(ListTeamsQuery query, CancellationToken ct)
    {
        var all = _db.Teams
            .Select(t => new TeamDto(t.Id, t.Name))
            .ToList();

        return Task.FromResult(new PagedResult<TeamDto>
        {
            Data = all,
            TotalRecords = all.Count,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(all.Count / (double)Math.Max(1, query.PageSize))
        });
    }
}

internal sealed class InMemoryPlayerQueryHandlers :
    IQueryHandler<GetPlayerByIdQuery, PlayerDto?>,
    IQueryHandler<ListPlayersQuery, PagedResult<PlayerDto>>
{
    private readonly AppDbContext _db;
    public InMemoryPlayerQueryHandlers(AppDbContext db) => _db = db;

    public Task<PlayerDto?> Handle(GetPlayerByIdQuery query, CancellationToken ct)
    {
        var row = _db.Players
            .Join(_db.Teams, p => p.TeamId, t => t.Id, (p, t) => new { p, t })
            .Where(x => x.p.Id == query.PlayerId)
            .Select(x => new PlayerDto(x.p.Id, x.p.TeamId, x.t.Name, x.p.FullName, x.p.JerseyNumber, x.p.GoalsScored, x.p.GoalsAgainst))
            .FirstOrDefault();

        return Task.FromResult<PlayerDto?>(row);
    }

    public Task<PagedResult<PlayerDto>> Handle(ListPlayersQuery query, CancellationToken ct)
    {
        var all = _db.Players
            .Join(_db.Teams, p => p.TeamId, t => t.Id, (p, t) => new { p, t })
            .Select(x => new PlayerDto(x.p.Id, x.p.TeamId, x.t.Name, x.p.FullName, x.p.JerseyNumber, x.p.GoalsScored, x.p.GoalsAgainst))
            .ToList();

        return Task.FromResult(new PagedResult<PlayerDto>
        {
            Data = all,
            TotalRecords = all.Count,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(all.Count / (double)Math.Max(1, query.PageSize))
        });
    }
}

internal sealed class InMemoryMatchQueryHandlers :
    IQueryHandler<GetMatchByIdQuery, MatchDto?>,
    IQueryHandler<ListMatchesQuery, PagedResult<MatchDto>>
{
    private readonly AppDbContext _db;
    public InMemoryMatchQueryHandlers(AppDbContext db) => _db = db;

    private static MatchDto Map(Match m, string home, string away) =>
        new(m.Id, m.HomeTeamId, home, m.AwayTeamId, away,
            m.MatchDateUtc, m.Status.ToString(), m.HomeScore, m.AwayScore);

    public Task<MatchDto?> Handle(GetMatchByIdQuery query, CancellationToken ct)
    {
        var row = (from m in _db.Matches
                   join h in _db.Teams on m.HomeTeamId equals h.Id
                   join a in _db.Teams on m.AwayTeamId equals a.Id
                   where m.Id == query.MatchId
                   select Map(m, h.Name, a.Name))
                  .FirstOrDefault();

        return Task.FromResult<MatchDto?>(row);
    }

    public Task<PagedResult<MatchDto>> Handle(ListMatchesQuery query, CancellationToken ct)
    {
        var all = (from m in _db.Matches
                   join h in _db.Teams on m.HomeTeamId equals h.Id
                   join a in _db.Teams on m.AwayTeamId equals a.Id
                   select Map(m, h.Name, a.Name))
                  .ToList();

        return Task.FromResult(new PagedResult<MatchDto>
        {
            Data = all,
            TotalRecords = all.Count,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = (int)Math.Ceiling(all.Count / (double)Math.Max(1, query.PageSize))
        });
    }
}

internal sealed class InMemoryStandingQueryHandlers :
    IQueryHandler<GetStandingsQuery, PagedResult<StandingDto>>,
    IQueryHandler<GetTopScorersQuery, PagedResult<TopScorerDto>>
{
    public Task<PagedResult<StandingDto>> Handle(GetStandingsQuery query, CancellationToken ct) =>
        Task.FromResult(new PagedResult<StandingDto>
        {
            Data = Array.Empty<StandingDto>(),
            TotalRecords = 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = 0
        });

    public Task<PagedResult<TopScorerDto>> Handle(GetTopScorersQuery query, CancellationToken ct) =>
        Task.FromResult(new PagedResult<TopScorerDto>
        {
            Data = Array.Empty<TopScorerDto>(),
            TotalRecords = 0,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalPages = 0
        });
}
