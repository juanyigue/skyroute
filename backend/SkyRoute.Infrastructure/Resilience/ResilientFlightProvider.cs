using Polly;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Infrastructure.Resilience;

public sealed class ResilientFlightProvider(IFlightProvider inner, ResiliencePipeline pipeline) : IFlightProvider
{
    public string ProviderName => inner.ProviderName;

    public async Task<IReadOnlyList<FlightOffer>> SearchAsync(
        string origin,
        string destination,
        DateOnly departureDate,
        int passengers,
        CabinClass cabin,
        CancellationToken ct = default)
    {
        return await pipeline.ExecuteAsync(
            async token => await inner.SearchAsync(origin, destination, departureDate, passengers, cabin, token),
            ct);
    }
}
