using SkyRoute.Domain.Enums;
using SkyRoute.Domain.ValueObjects;

namespace SkyRoute.Application.UseCases.CreateBooking;

public sealed record CreateBookingCommand(
    IReadOnlyList<PassengerInfo> Passengers,
    DocumentType DocumentType,
    string Provider,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTimeOffset DepartureUtc,
    DateTimeOffset ArrivalUtc,
    CabinClass Cabin,
    decimal TotalPrice);
