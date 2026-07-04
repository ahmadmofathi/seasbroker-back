using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Seasbroker.Infrastructure.Persistence;
using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.Constants;
using Seasbroker.Modules.Identity.Application.DTOs;
using Seasbroker.Modules.Identity.Application.Services;
using Seasbroker.Modules.Identity.Infrastructure;
using Seasbroker.Modules.Identity.Infrastructure.Options;

namespace Seasbroker.Modules.Identity;

public static class DependencyInjection
{
    private static readonly JsonSerializerOptions AuthJsonOptions = new(JsonSerializerDefaults.Web);

    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<SuperuserSeedOptions>(configuration.GetSection(SuperuserSeedOptions.SectionName));
        services.AddSingleton<IValidateOptions<SuperuserSeedOptions>, SuperuserSeedOptionsValidator>();

        // Identity for user/role management only — no cookie authentication schemes.
        services
            .AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<SeasbrokerDbContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddHostedService<SuperuserSeeder>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    RoleClaimType = ClaimTypes.Role,
                    NameClaimType = ClaimTypes.NameIdentifier,
                };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        // Prevent the default WWW-Authenticate challenge body and any host redirects.
                        context.HandleResponse();

                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/json";

                        var payload = new AuthErrorResponse
                        {
                            Message = string.IsNullOrWhiteSpace(context.ErrorDescription)
                                ? "The request requires valid authorization token."
                                : context.ErrorDescription,
                            Status = StatusCodes.Status401Unauthorized,
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, AuthJsonOptions));
                    },
                    OnForbidden = async context =>
                    {
                        if (context.Response.HasStarted)
                        {
                            return;
                        }

                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var payload = new AuthErrorResponse
                        {
                            Message = "Access denied.",
                            Status = StatusCodes.Status403Forbidden,
                        };

                        await context.Response.WriteAsync(JsonSerializer.Serialize(payload, AuthJsonOptions));
                    },
                };
            });

        // Defense in depth: if any cookie scheme is registered later, never redirect API clients.
        services.ConfigureApplicationCookie(options =>
        {
            options.Events.OnRedirectToLogin = context =>
            {
                if (IsApiOrHubRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                if (IsApiOrHubRequest(context.Request))
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    return Task.CompletedTask;
                }

                context.Response.Redirect(context.RedirectUri);
                return Task.CompletedTask;
            };
        });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(SeasbrokerIdentityConstants.SuperuserPolicy, policy =>
                policy.RequireRole(SeasbrokerIdentityConstants.SuperuserRole));
        });

        return services;
    }

    public static IMvcBuilder AddIdentityModuleControllers(this IMvcBuilder mvcBuilder)
    {
        return mvcBuilder.AddApplicationPart(typeof(DependencyInjection).Assembly);
    }

    private static bool IsApiOrHubRequest(HttpRequest request)
    {
        var path = request.Path;
        return path.StartsWithSegments("/api") || path.StartsWithSegments("/hubs");
    }
}
