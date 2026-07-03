namespace Seasbroker.Modules.Cargo.Application.Exceptions;

public class CargoException : Exception
{
    public CargoException(string message, int statusCode, string? details = null)
        : base(message)
    {
        StatusCode = statusCode;
        Details = details;
    }

    public int StatusCode { get; }

    public string? Details { get; }
}
