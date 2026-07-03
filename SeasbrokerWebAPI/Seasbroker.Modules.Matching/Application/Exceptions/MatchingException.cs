namespace Seasbroker.Modules.Matching.Application.Exceptions;

public class MatchingException : Exception
{
    public MatchingException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
