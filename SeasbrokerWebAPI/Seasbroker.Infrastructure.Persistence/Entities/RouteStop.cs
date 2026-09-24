namespace Seasbroker.Infrastructure.Persistence.Entities;

/// <summary>One port call on a vessel's route, in sailing order. The first stop is the vessel's next port.</summary>
public class RouteStop
{
    public string Port { get; set; } = string.Empty;

    public DateTime Eta { get; set; }
}
