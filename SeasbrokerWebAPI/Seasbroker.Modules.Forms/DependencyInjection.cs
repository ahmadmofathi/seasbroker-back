using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Seasbroker.Modules.Forms.Application.Services;
using Seasbroker.Modules.Forms.Infrastructure;

namespace Seasbroker.Modules.Forms;

public static class DependencyInjection
{
    public static IServiceCollection AddFormsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<FormsStorageOptions>(configuration.GetSection("Forms:Storage"));

        services.AddScoped<IFormBuilderService, FormBuilderService>();
        services.AddScoped<IFormSubmissionService, FormSubmissionService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        services.AddHostedService<FormSeeder>();

        services.AddExceptionHandler<FormsExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static IMvcBuilder AddFormsModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }
}

public static class FormsModuleApplicationBuilderExtensions
{
    public static WebApplication UseFormsModule(this WebApplication app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
