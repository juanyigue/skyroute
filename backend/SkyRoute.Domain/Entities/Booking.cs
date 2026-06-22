using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Entities;

public sealed class Booking
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string PassengerName { get; init; } = default!;
    public DocumentType DocumentType { get; init; }
    public string DocumentNumber { get; init; } = default!;
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
    public DateTimeOffset BookedAt { get; init; } = DateTimeOffset.UtcNow;
}
