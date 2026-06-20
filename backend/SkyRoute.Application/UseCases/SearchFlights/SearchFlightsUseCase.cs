using Microsoft.Extensions.Logging;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;

namespace SkyRoute.Application.UseCases.SearchFlights;

public sealed class SearchFlightsUseCase(
    IEnumerable<IFlightProvider> providers,
    ILogger<SearchFlightsUseCase> logger)
{
    public async Task<SearchFlightsResult> ExecuteAsync(SearchFlightsQuery query, CancellationToken ct = default)
    {
        var tasks = providers.Select(p => FetchSafe(p, query, ct)).ToList();
        var results = await Task.WhenAll(tasks);

        var offers = new List<FlightOffer>();
        var errors = new List<string>();

        foreach (var (providerOffers, error) in results)
        {
            if (error is not null)
                errors.Add(error);
            else
                offers.AddRange(providerOffers!);
        }

        return new SearchFlightsResult(offers, errors);
    }

    private async Task<(IReadOnlyList<FlightOffer>? Offers, string? Error)> FetchSafe(
        IFlightProvider provider, SearchFlightsQuery query, CancellationToken ct)
    {
        try
        {
            var offers = await provider.SearchAsync(
                query.Origin, query.Destination, query.DepartureDate,
                query.Passengers, query.Cabin, ct);
            return (offers, null);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Provider {Provider} failed", provider.ProviderName);
            return (null, $"{provider.ProviderName}: {ex.Message}");
        }
    }
}
