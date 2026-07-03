using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Approval.Application.Abstractions;
using Seasbroker.Modules.Approval.Application.Commands;
using Seasbroker.Modules.Approval.Application.DTOs;
using Seasbroker.Modules.Approval.Application.Handlers.Commands;
using Seasbroker.Modules.Approval.Application.Handlers.Queries;
using Seasbroker.Modules.Approval.Application.Queries;
using Seasbroker.Modules.Approval.Application.Services;
using Seasbroker.Modules.Approval.Application.Validators;
using Seasbroker.Modules.Approval.Infrastructure;

namespace Seasbroker.Modules.Approval;

public static class DependencyInjection
{
    public static IServiceCollection AddApprovalModule(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddValidatorsFromAssemblyContaining<ApproveMatchCommandValidator>();

        services.AddScoped<ICommandHandler<ApproveMatchCommand, MatchApprovalRecordDto>, ApproveMatchCommandHandler>();
        services.AddScoped<ICommandHandler<RejectMatchCommand, MatchApprovalRecordDto>, RejectMatchCommandHandler>();
        services.AddScoped<ICommandHandler<CancelMatchCommand, MatchApprovalRecordDto>, CancelMatchCommandHandler>();
        services.AddScoped<ICommandHandler<CompleteMatchCommand, MatchApprovalRecordDto>, CompleteMatchCommandHandler>();

        services.AddScoped<IQueryHandler<GetPendingApprovalMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>>, GetPendingApprovalMatchesQueryHandler>();
        services.AddScoped<IQueryHandler<GetApprovedMatchesQuery, PocketBaseListResponse<MatchApprovalRecordDto>>, GetApprovedMatchesQueryHandler>();

        services.AddScoped<IMatchApprovalService, MatchApprovalService>();
        services.AddScoped<IMatchApprovalQueryService, MatchApprovalQueryService>();
        services.AddScoped<IMatchApprovalWorkflowService, MatchApprovalWorkflowService>();

        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

        services.AddExceptionHandler<ApprovalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddApprovalModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}

public static class ApprovalModuleApplicationBuilderExtensions
{
    public static WebApplication UseApprovalModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
