using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Seasbroker.Modules.Identity.Infrastructure.Options;

public sealed class SuperuserSeedOptionsValidator : IValidateOptions<SuperuserSeedOptions>
{
    private const string DefaultWeakPassword = "adminadmin";
    private const int ProductionMinimumPasswordLength = 12;

    private readonly IHostEnvironment _environment;

    public SuperuserSeedOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, SuperuserSeedOptions options)
    {
        if (_environment.IsDevelopment())
        {
            if (string.IsNullOrWhiteSpace(options.Email) || string.IsNullOrWhiteSpace(options.Password))
            {
                return ValidateOptionsResult.Fail(
                    "Identity:Superuser:Email and Identity:Superuser:Password must be configured.");
            }

            return ValidateOptionsResult.Success;
        }

        if (string.IsNullOrWhiteSpace(options.Email))
        {
            return ValidateOptionsResult.Fail("Identity:Superuser:Email is required in non-Development environments.");
        }

        if (string.Equals(options.Password, DefaultWeakPassword, StringComparison.Ordinal))
        {
            return ValidateOptionsResult.Fail("Identity:Superuser:Password must not use the default weak value.");
        }

        if (string.IsNullOrWhiteSpace(options.Password) || options.Password.Length < ProductionMinimumPasswordLength)
        {
            return ValidateOptionsResult.Fail(
                $"Identity:Superuser:Password must be at least {ProductionMinimumPasswordLength} characters in non-Development environments.");
        }

        return ValidateOptionsResult.Success;
    }
}
