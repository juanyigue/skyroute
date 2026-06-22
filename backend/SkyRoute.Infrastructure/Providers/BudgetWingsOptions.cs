namespace SkyRoute.Infrastructure.Providers;

public sealed class BudgetWingsOptions
{
    public bool SimulateFailure { get; set; }
    /// <summary>
    /// Artificially delays SearchAsync by this many seconds. Use to trigger timeout/retry scenarios.
    /// </summary>
    public int SimulateDelaySeconds { get; set; }
}
