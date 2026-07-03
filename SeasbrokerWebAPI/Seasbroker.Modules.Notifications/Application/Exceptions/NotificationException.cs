namespace Seasbroker.Modules.Notifications.Application.Exceptions;

public class NotificationException : Exception
{
    public NotificationException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
