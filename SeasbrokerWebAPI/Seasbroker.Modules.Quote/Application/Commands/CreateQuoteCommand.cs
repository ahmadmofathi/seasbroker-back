namespace Seasbroker.Modules.Quote.Application.Commands;

public sealed record CreateQuoteCommand(
    string CargoType,
    double Weight,
    string DeparturePort,
    string DepartureTime,
    string ArrivalPort,
    string ArrivalTime,
    string Dimensions,
    string? AdditionalInfo,
    string Fname,
    string Lname,
    string Email,
    string PhoneNumber);
