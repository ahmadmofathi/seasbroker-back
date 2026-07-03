namespace Seasbroker.Modules.Quote.Application.Exceptions;

public class QuoteException : Exception
{
    public QuoteException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
