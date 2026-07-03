using Microsoft.AspNetCore.Identity;

namespace Seasbroker.Infrastructure.Persistence.Entities;

public class User : IdentityUser<Guid>
{
    public bool Verified { get; set; }

    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}
