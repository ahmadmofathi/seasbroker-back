using Seasbroker.Infrastructure.Persistence.Entities;
using Seasbroker.Modules.Identity.Application.Constants;
using Seasbroker.Modules.Identity.Application.DTOs;

namespace Seasbroker.Modules.Identity.Application.Mapping;

public static class UserMapper
{
    public static UserDto ToDto(User user, IReadOnlyList<string> roles)
    {
        return new UserDto
        {
            Id = user.Id.ToString(),
            Email = user.Email ?? string.Empty,
            Verified = user.Verified,
            Roles = roles,
            Created = user.Created,
            Updated = user.Updated,
        };
    }

    public static PocketBaseSuperuserRecord ToPocketBaseRecord(User user)
    {
        return new PocketBaseSuperuserRecord
        {
            Id = user.Id.ToString(),
            CollectionId = SeasbrokerIdentityConstants.SuperusersCollectionName,
            CollectionName = SeasbrokerIdentityConstants.SuperusersCollectionName,
            Created = user.Created,
            Updated = user.Updated,
            Email = user.Email ?? string.Empty,
            Verified = user.Verified,
        };
    }
}
