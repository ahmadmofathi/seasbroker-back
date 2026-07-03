using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Cargo.Application.Abstractions;
using Seasbroker.Modules.Cargo.Application.Commands;
using Seasbroker.Modules.Cargo.Application.DTOs;
using Seasbroker.Modules.Cargo.Application.Handlers.Commands;
using Seasbroker.Modules.Cargo.Application.Handlers.Queries;
using Seasbroker.Modules.Cargo.Application.Queries;
using Seasbroker.Modules.Cargo.Application.Services;
using Seasbroker.Modules.Cargo.Application.Validators;
using Seasbroker.Modules.Cargo.Infrastructure;

namespace Seasbroker.Modules.Cargo;

public static class DependencyInjection
{
    public static IServiceCollection AddCargoModule(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateCargoListingCommandValidator>();

        services.AddScoped<ICommandHandler<CreateCargoListingCommand, CargoListingRecordDto>, CreateCargoListingCommandHandler>();
        services.AddScoped<ICommandHandler<UpdateCargoListingCommand, CargoListingRecordDto>, UpdateCargoListingCommandHandler>();
        services.AddScoped<ICommandHandler<PromoteQuoteToCargoCommand, CargoListingRecordDto>, PromoteQuoteToCargoCommandHandler>();
        services.AddScoped<ICommandHandler<CloseCargoListingCommand, CargoListingRecordDto>, CloseCargoListingCommandHandler>();
        services.AddScoped<ICommandHandler<CancelCargoListingCommand, CargoListingRecordDto>, CancelCargoListingCommandHandler>();

        services.AddScoped<IQueryHandler<GetCargoListingsQuery, PocketBaseListResponse<CargoListingRecordDto>>, GetCargoListingsQueryHandler>();
        services.AddScoped<IQueryHandler<GetCargoListingByIdQuery, CargoListingRecordDto>, GetCargoListingByIdQueryHandler>();
        services.AddScoped<IQueryHandler<GetOpenCargoForMatchingQuery, IReadOnlyList<CargoListingRecordDto>>, GetOpenCargoForMatchingQueryHandler>();
        services.AddScoped<IQueryHandler<GetCargoByQuoteIdQuery, CargoListingRecordDto?>, GetCargoByQuoteIdQueryHandler>();

        services.AddScoped<ICargoListingService, CargoListingService>();
        services.AddScoped<IQuotePromotionService, QuotePromotionService>();
        services.AddScoped<ICargoQueryService, CargoQueryService>();

        services.AddExceptionHandler<CargoExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddCargoModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}

public static class CargoModuleApplicationBuilderExtensions
{
    public static WebApplication UseCargoModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
