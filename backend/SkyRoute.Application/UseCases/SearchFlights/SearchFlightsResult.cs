using SkyRoute.Domain.Entities;

namespace SkyRoute.Application.UseCases.SearchFlights;

public sealed record SearchFlightsResult(
    IReadOnlyList<FlightOffer> Offers,
    IReadOnlyList<string> ProviderErrors);
