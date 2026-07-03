namespace Seasbroker.Modules.Vessel.Application.Exceptions;

public class VesselException : Exception
{
    public VesselException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
