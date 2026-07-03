namespace Seasbroker.Modules.Approval.Application.Exceptions;

public class ApprovalException : Exception
{
    public ApprovalException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
