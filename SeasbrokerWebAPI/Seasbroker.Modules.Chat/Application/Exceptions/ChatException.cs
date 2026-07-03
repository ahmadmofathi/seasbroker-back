namespace Seasbroker.Modules.Chat.Application.Exceptions;

public class ChatException : Exception
{
    public ChatException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
