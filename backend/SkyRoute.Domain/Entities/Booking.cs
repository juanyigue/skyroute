using SkyRoute.Domain.Enums;
using SkyRoute.Domain.ValueObjects;

namespace SkyRoute.Domain.Entities;

public sealed class Booking
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public IReadOnlyList<PassengerInfo> PassengerDetails { get; init; } = [];
    public DocumentType DocumentType { get; init; }
    public string Provider { get; init; } = default!;
    public string FlightNumber { get; init; } = default!;
    public string Origin { get; init; } = default!;
    public string Destination { get; init; } = default!;
    public DateTimeOffset DepartureUtc { get; init; }
    public DateTimeOffset ArrivalUtc { get; init; }
    public CabinClass Cabin { get; init; }
    public decimal TotalPrice { get; init; }
    public string Currency { get; init; } = "USD";
    public DateTimeOffset BookedAt { get; init; } = DateTimeOffset.UtcNow;
}
