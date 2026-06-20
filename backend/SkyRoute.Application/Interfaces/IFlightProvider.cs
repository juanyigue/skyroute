using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.Interfaces;

public interface IFlightProvider
{
    string ProviderName { get; }
    Task<IReadOnlyList<FlightOffer>> SearchAsync(
        string origin,
        string destination,
        DateOnly departureDate,
        int passengers,
        CabinClass cabin,
        CancellationToken ct = default);
}
