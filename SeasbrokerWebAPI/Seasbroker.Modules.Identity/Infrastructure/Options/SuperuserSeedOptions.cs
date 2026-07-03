namespace Seasbroker.Modules.Identity.Infrastructure.Options;

public class SuperuserSeedOptions
{
    public const string SectionName = "Identity:Superuser";

    public string Email { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
