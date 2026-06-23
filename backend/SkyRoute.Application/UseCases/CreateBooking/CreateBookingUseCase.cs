using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Services;

namespace SkyRoute.Application.UseCases.CreateBooking;

public sealed class CreateBookingUseCase(
    IBookingRepository bookings,
    IAirportRepository airports)
{
    public Booking Execute(CreateBookingCommand command)
    {
        if (command.Passengers.Count == 0)
            throw new ArgumentException("At least one passenger is required.");

        var origin = airports.FindByCode(command.Origin)
            ?? throw new ArgumentException($"Unknown airport: {command.Origin}");
        var destination = airports.FindByCode(command.Destination)
            ?? throw new ArgumentException($"Unknown airport: {command.Destination}");

        var expectedDocument = RouteClassifier.RequiredDocument(origin.CountryCode, destination.CountryCode);
        if (command.DocumentType != expectedDocument)
            throw new ArgumentException(
                $"Route {command.Origin}→{command.Destination} requires {expectedDocument}, got {command.DocumentType}.");

        var booking = new Booking
        {
            PassengerDetails = command.Passengers,
            DocumentType = command.DocumentType,
            Provider = command.Provider,
            FlightNumber = command.FlightNumber,
            Origin = command.Origin,
            Destination = command.Destination,
            DepartureUtc = command.DepartureUtc,
            ArrivalUtc = command.ArrivalUtc,
            Cabin = command.Cabin,
            TotalPrice = command.TotalPrice
        };

        bookings.Add(booking);
        return booking;
    }
}
