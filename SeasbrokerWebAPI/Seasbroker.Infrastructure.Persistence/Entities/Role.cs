using Microsoft.AspNetCore.Identity;

namespace Seasbroker.Infrastructure.Persistence.Entities;

public class Role : IdentityRole<Guid>
{
    public DateTime Created { get; set; }

    public DateTime Updated { get; set; }
}
