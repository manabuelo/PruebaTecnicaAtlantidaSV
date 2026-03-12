using Microsoft.Extensions.DependencyInjection;
using MundialCorporativo.Application.Abstractions.CQRS;
using MundialCorporativo.Application.Matches;
using MundialCorporativo.Application.Players;
using MundialCorporativo.Application.Standings;
using MundialCorporativo.Application.Teams;
using MundialCorporativo.Domain.Common;

namespace MundialCorporativo.Api.DependencyInjection;

public static class ApplicationHandlersRegistration
{
    public static IServiceCollection AddApplicationHandlers(this IServiceCollection services)
    {
        services.AddScoped<TeamCommandHandlers>();
        services.AddScoped<ICommandHandler<CreateTeamCommand, Result<Guid>>, TeamCommandHandlers>();
        services.AddScoped<ICommandHandler<UpdateTeamCommand, Result>, TeamCommandHandlers>();
        services.AddScoped<ICommandHandler<PatchTeamCommand, Result>, TeamCommandHandlers>();
        services.AddScoped<ICommandHandler<DeleteTeamCommand, Result>, TeamCommandHandlers>();

        services.AddScoped<TeamQueryHandlers>();
        services.AddScoped<IQueryHandler<GetTeamByIdQuery, MundialCorporativo.Application.DTOs.TeamDto?>, TeamQueryHandlers>();
        services.AddScoped<IQueryHandler<ListTeamsQuery, MundialCorporativo.Application.Common.PagedResult<MundialCorporativo.Application.DTOs.TeamDto>>, TeamQueryHandlers>();

        services.AddScoped<PlayerCommandHandlers>();
        services.AddScoped<ICommandHandler<CreatePlayerCommand, Result<Guid>>, PlayerCommandHandlers>();
        services.AddScoped<ICommandHandler<UpdatePlayerCommand, Result>, PlayerCommandHandlers>();
        services.AddScoped<ICommandHandler<PatchPlayerCommand, Result>, PlayerCommandHandlers>();
        services.AddScoped<ICommandHandler<DeletePlayerCommand, Result>, PlayerCommandHandlers>();

        services.AddScoped<PlayerQueryHandlers>();
        services.AddScoped<IQueryHandler<GetPlayerByIdQuery, MundialCorporativo.Application.DTOs.PlayerDto?>, PlayerQueryHandlers>();
        services.AddScoped<IQueryHandler<ListPlayersQuery, MundialCorporativo.Application.Common.PagedResult<MundialCorporativo.Application.DTOs.PlayerDto>>, PlayerQueryHandlers>();

        services.AddScoped<MatchCommandHandlers>();
        services.AddScoped<ICommandHandler<CreateMatchCommand, Result<Guid>>, MatchCommandHandlers>();
        services.AddScoped<ICommandHandler<RegisterMatchResultCommand, Result>, MatchCommandHandlers>();
        services.AddScoped<ICommandHandler<PatchMatchCommand, Result>, MatchCommandHandlers>();
        services.AddScoped<ICommandHandler<DeleteMatchCommand, Result>, MatchCommandHandlers>();

        services.AddScoped<MatchQueryHandlers>();
        services.AddScoped<IQueryHandler<GetMatchByIdQuery, MundialCorporativo.Application.DTOs.MatchDto?>, MatchQueryHandlers>();
        services.AddScoped<IQueryHandler<ListMatchesQuery, MundialCorporativo.Application.Common.PagedResult<MundialCorporativo.Application.DTOs.MatchDto>>, MatchQueryHandlers>();

        services.AddScoped<StandingQueryHandlers>();
        services.AddScoped<IQueryHandler<GetStandingsQuery, MundialCorporativo.Application.Common.PagedResult<MundialCorporativo.Application.DTOs.StandingDto>>, StandingQueryHandlers>();
        services.AddScoped<IQueryHandler<GetTopScorersQuery, MundialCorporativo.Application.Common.PagedResult<MundialCorporativo.Application.DTOs.TopScorerDto>>, StandingQueryHandlers>();

        return services;
    }
}
