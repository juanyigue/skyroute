using Microsoft.Extensions.Options;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Infrastructure.Providers;

public sealed class BudgetWingsProvider(IOptions<BudgetWingsOptions> options) : IFlightProvider
{
    private const decimal Discount = 0.10m;
    private const decimal FloorPricePerPax = 29.99m;

    public string ProviderName => "BudgetWings";

    public Task<IReadOnlyList<FlightOffer>> SearchAsync(
        string origin,
        string destination,
        DateOnly departureDate,
        int passengers,
        CabinClass cabin,
        CancellationToken ct = default)
    {
        if (options.Value.SimulateFailure)
            throw new InvalidOperationException("BudgetWings service is temporarily unavailable.");

        var cabinMultiplier = (int)cabin;
        var basePrice = GenerateBasePrice(origin, destination);
        var discounted = basePrice * cabinMultiplier * (1 - Discount);
        var pricePerPax = Math.Max(discounted, FloorPricePerPax);
        var total = Math.Round(pricePerPax * passengers, 2, MidpointRounding.AwayFromZero);

        var departure = departureDate.ToDateTime(new TimeOnly(10, 30), DateTimeKind.Utc);

        IReadOnlyList<FlightOffer> offers =
        [
            new FlightOffer
            {
                Provider = ProviderName,
                FlightNumber = $"BW{Math.Abs(HashCode.Combine(origin, destination, departureDate)) % 9000 + 1000}",
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
        var hash = Math.Abs(HashCode.Combine(origin.ToUpperInvariant(), destination.ToUpperInvariant()));
        return 150m + hash % 500m;
    }

    private static double EstimateFlightHours(string origin, string destination) =>
        Math.Abs(HashCode.Combine(origin, destination)) % 12 + 2;
}
