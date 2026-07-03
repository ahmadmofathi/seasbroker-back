namespace Seasbroker.Modules.Matching.Infrastructure.Options;

public class MatchingOptions
{
    public const string SectionName = "Matching";

    public decimal MinScore { get; set; } = 60;

    public int MaxProposalsPerCargo { get; set; } = 5;

    public int ProposalTtlHours { get; set; } = 72;

    public int ExpiryWorkerIntervalMinutes { get; set; } = 15;
}
