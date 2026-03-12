using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MundialCorporativo.Application.Abstractions.Events;
using MundialCorporativo.Application.Abstractions.Idempotency;
using MundialCorporativo.Application.Abstractions.Persistence;
using MundialCorporativo.Application.Abstractions.Read;
using MundialCorporativo.Infrastructure.Persistence;
using MundialCorporativo.Infrastructure.Persistence.Repositories;
using MundialCorporativo.Infrastructure.Read;
using MundialCorporativo.Infrastructure.Seed;
using MundialCorporativo.Infrastructure.Services;

namespace MundialCorporativo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection no configurada.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IPlayerRepository, PlayerRepository>();
        services.AddScoped<IMatchRepository, MatchRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IReadDbConnectionFactory>(_ => new ReadDbConnectionFactory(connectionString));
        services.AddScoped<IDomainEventLogger, DomainEventLogger>();
        services.AddScoped<IIdempotencyService, IdempotencyService>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
