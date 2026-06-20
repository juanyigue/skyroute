using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Entities;

public sealed class FlightOffer
{
    public string Provider { get; init; } = default!;
    public string FlightNumber { get; init; } = default!;
    public string Origin { get; init; } = default!;
    public string Destination { get; init; } = default!;
    public DateTimeOffset DepartureUtc { get; init; }
    public DateTimeOffset ArrivalUtc { get; init; }
    public CabinClass Cabin { get; init; }
    public int Passengers { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = "USD";
}
