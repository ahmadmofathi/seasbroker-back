using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Vessel.Application.Abstractions;
using Seasbroker.Modules.Vessel.Application.Commands;
using Seasbroker.Modules.Vessel.Application.DTOs;
using Seasbroker.Modules.Vessel.Application.Handlers.Commands;
using Seasbroker.Modules.Vessel.Application.Handlers.Queries;
using Seasbroker.Modules.Vessel.Application.Queries;
using Seasbroker.Modules.Vessel.Application.Services;
using Seasbroker.Modules.Vessel.Application.Validators;
using Seasbroker.Modules.Vessel.Infrastructure;

namespace Seasbroker.Modules.Vessel;

public static class DependencyInjection
{
    public static IServiceCollection AddVesselModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateVesselCommandValidator>();

        services.AddScoped<ICommandHandler<CreateVesselCommand, VesselRecordDto>, CreateVesselCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateVesselCommand, VesselRecordDto>, UpdateVesselCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateVesselCommand>, DeactivateVesselCommandHandler>();
        services.AddScoped<ICommandHandler<CreateVesselAvailabilityCommand, VesselAvailabilityRecordDto>, CreateVesselAvailabilityCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateVesselAvailabilityCommand, VesselAvailabilityRecordDto>, UpdateVesselAvailabilityCommandHandler>();
        services.AddScoped<ICommandHandler<DeactivateVesselAvailabilityCommand>, DeactivateVesselAvailabilityCommandHandler>();

        services.AddScoped<IQueryHandler<GetVesselsQuery, PocketBaseListResponse<VesselRecordDto>>, GetVesselsQueryHandler>();
        services.AddScoped<IQueryHandler<GetVesselByIdQuery, VesselRecordDto>, GetVesselByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetVesselAvailabilitiesQuery, IReadOnlyList<VesselAvailabilityRecordDto>>, GetVesselAvailabilitiesQueryHandler>();
        services.AddScoped<IQueryHandler<GetAvailableVesselsQuery, IReadOnlyList<VesselRecordDto>>, GetAvailableVesselsQueryHandler>();

        services.AddScoped<IVesselService, VesselService>();
        services.AddScoped<IVesselAvailabilityService, VesselAvailabilityService>();
        services.AddScoped<IVesselQueryService, VesselQueryService>();

        services.AddExceptionHandler<VesselExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddVesselModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}

public static class VesselModuleApplicationBuilderExtensions
{
    public static WebApplication UseVesselModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
