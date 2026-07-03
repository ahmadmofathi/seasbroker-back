namespace Seasbroker.Modules.Quote.Application.Commands;

public sealed record CreateCustomerCommand(
    string Email,
    string PhoneNumber,
    string FirstName,
    string LastName);
