namespace Seasbroker.Modules.Notifications.Application.Constants;

public static class NotificationConstants
{
    public const string SuperuserPolicy = "Superuser";
}

public static class NotificationHubGroups
{
    public const string Admin = "admin:notifications";

    public static string ForUser(Guid userId) => $"user:{userId:D}";
}

public static class NotificationHubMethods
{
    public const string ReceiveNotification = "ReceiveNotification";
}
