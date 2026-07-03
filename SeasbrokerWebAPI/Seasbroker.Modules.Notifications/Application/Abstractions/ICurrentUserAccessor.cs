namespace Seasbroker.Modules.Notifications.Application.Abstractions;

public interface ICurrentUserAccessor
{
    Guid GetRequiredUserId();
}
