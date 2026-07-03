using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Quote.Application.Services;
using Seasbroker.Modules.Quote.Infrastructure;

namespace Seasbroker.Modules.Quote;

public static class DependencyInjection
{
    public static IServiceCollection AddQuoteModule(this IServiceCollection services)
    {
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddExceptionHandler<QuoteExceptionHandler>();

        return services;
    }

    public static IMvcBuilder AddQuoteModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}
