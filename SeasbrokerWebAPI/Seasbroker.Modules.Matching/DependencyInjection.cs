using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Matching.Application.Abstractions;
using Seasbroker.Modules.Matching.Application.Commands;
using Seasbroker.Modules.Matching.Application.DTOs;
using Seasbroker.Modules.Matching.Application.Handlers.Commands;
using Seasbroker.Modules.Matching.Application.Handlers.Queries;
using Seasbroker.Modules.Matching.Application.Queries;
using Seasbroker.Modules.Matching.Application.Services;
using Seasbroker.Modules.Matching.Application.Validators;
using Seasbroker.Modules.Matching.Infrastructure;
using Seasbroker.Modules.Matching.Infrastructure.Options;

namespace Seasbroker.Modules.Matching;

public static class DependencyInjection
{
    public static IServiceCollection AddMatchingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<MatchingOptions>(configuration.GetSection(MatchingOptions.SectionName));

        services.AddValidatorsFromAssemblyContaining<RunMatchingCommandValidator>();

        services.AddScoped<ICommandHandler<RunMatchingCommand, MatchingRunResultDto>, RunMatchingCommandHandler>();
        services.AddScoped<ICommandHandler<CreateManualMatchCommand, MatchRecordDto>, CreateManualMatchCommandHandler>();
        services.AddScoped<ICommandHandler<ExpireMatchCommand, MatchRecordDto>, ExpireMatchCommandHandler>();
        services.AddScoped<ICommandHandler<CancelMatchCommand, MatchRecordDto>, CancelMatchCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateMatchingRuleCommand, MatchingRuleRecordDto>, UpdateMatchingRuleCommandHandler>();

        services.AddScoped<IQueryHandler<GetMatchesQuery, PocketBaseListResponse<MatchRecordDto>>, GetMatchesQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchByIdQuery, MatchRecordDto>, GetMatchByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetMatchingRulesQuery, IReadOnlyList<MatchingRuleRecordDto>>, GetMatchingRulesQueryHandler>();

        services.AddScoped<IMatchingEngineService, MatchingEngineService>();
        services.AddScoped<IMatchService, MatchService>();
        services.AddScoped<IMatchingRuleService, MatchingRuleService>();
        services.AddScoped<IMatchQueryService, MatchQueryService>();

        services.AddScoped<IMatchingRunService, MatchingRunService>();
        services.AddScoped<IMatchLifecycleService, MatchLifecycleService>();
        services.AddScoped<IMatchRecordsService, MatchRecordsService>();
        services.AddScoped<IMatchingRuleRecordsService, MatchingRuleRecordsService>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();

        services.AddHostedService<MatchingRuleSeeder>();
        services.AddHostedService<MatchExpiryHostedService>();

        services.AddExceptionHandler<MatchingExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddMatchingModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}

public static class MatchingModuleApplicationBuilderExtensions
{
    public static WebApplication UseMatchingModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
