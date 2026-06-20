using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Infrastructure.Providers;

public sealed class GlobalAirProvider : IFlightProvider
{
    private const decimal FuelSurcharge = 0.15m;

    public string ProviderName => "GlobalAir";

    public Task<IReadOnlyList<FlightOffer>> SearchAsync(
        string origin,
        string destination,
        DateOnly departureDate,
        int passengers,
        CabinClass cabin,
        CancellationToken ct = default)
    {
        var cabinMultiplier = (int)cabin;
        var basePrice = GenerateBasePrice(origin, destination);
        var pricePerPax = Math.Round(basePrice * cabinMultiplier * (1 + FuelSurcharge), 2, MidpointRounding.AwayFromZero);
        var total = pricePerPax * passengers;

        var departure = departureDate.ToDateTime(new TimeOnly(8, 0), DateTimeKind.Utc);

        IReadOnlyList<FlightOffer> offers =
        [
            new FlightOffer
            {
                Provider = ProviderName,
                FlightNumber = $"GA{Math.Abs(HashCode.Combine(origin, destination, departureDate)) % 9000 + 1000}",
                Origin = origin,
                Destination = destination,
                DepartureUtc = new DateTimeOffset(departure),
                ArrivalUtc = new DateTimeOffset(departure.AddHours(EstimateFlightHours(origin, destination))),
                Cabin = cabin,
                Passengers = passengers,
                TotalPrice = total,
                Currency = "USD"
            }
        ];

        return Task.FromResult(offers);
    }

    private static decimal GenerateBasePrice(string origin, string destination)
    {
        // Deterministic pseudo-price based on route hash
        var hash = Math.Abs(HashCode.Combine(origin.ToUpperInvariant(), destination.ToUpperInvariant()));
        return 150m + hash % 500m;
    }

    private static double EstimateFlightHours(string origin, string destination) =>
        Math.Abs(HashCode.Combine(origin, destination)) % 12 + 2;
}
