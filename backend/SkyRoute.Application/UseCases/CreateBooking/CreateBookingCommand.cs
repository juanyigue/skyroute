using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.UseCases.CreateBooking;

public sealed record CreateBookingCommand(
    string PassengerName,
    DocumentType DocumentType,
    string DocumentNumber,
    string Provider,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTimeOffset DepartureUtc,
    DateTimeOffset ArrivalUtc,
    CabinClass Cabin,
    int Passengers,
    decimal TotalPrice);
