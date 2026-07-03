namespace SeasbrokerWebAPI.IntegrationTests.Infrastructure;

internal static class IntegrationTestDefaults
{
    public const string SuperuserEmail = "admin@integration.seasbroker.test";

    public const string SuperuserPassword = "Integration_Test_12!";

    public const string DeparturePort = "Rotterdam";

    public const string ArrivalPort = "Singapore";

    public const string CargoType = "Bulk";

    public static DateTime DepartureTimeUtc => DateTime.UtcNow.AddDays(10);

    public static DateTime ArrivalTimeUtc => DateTime.UtcNow.AddDays(30);

    public static string DepartureTimeIso => DepartureTimeUtc.ToString("O");

    public static string ArrivalTimeIso => ArrivalTimeUtc.ToString("O");

    public static string ImoNumber() =>
        Random.Shared.Next(1_000_000, 10_000_000).ToString();
}
