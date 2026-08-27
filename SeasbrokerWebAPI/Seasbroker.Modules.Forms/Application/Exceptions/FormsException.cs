namespace Seasbroker.Modules.Forms.Application.Exceptions;

public class FormsException : Exception
{
    public FormsException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
