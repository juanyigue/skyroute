using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.CreateBooking;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.ValueObjects;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(
    CreateBookingUseCase createBooking,
    IBookingRepository bookings) : ControllerBase
{
    [HttpPost]
    public IActionResult Create([FromBody] CreateBookingRequest request)
    {
        try
        {
            var command = new CreateBookingCommand(
                request.Passengers.Select(p => new PassengerInfo(p.Name, p.Email, p.DocumentNumber)).ToList(),
                request.DocumentType,
                request.Provider,
                request.FlightNumber,
                request.Origin,
                request.Destination,
                request.DepartureUtc,
                request.ArrivalUtc,
                request.Cabin,
                request.TotalPrice);

            var booking = createBooking.Execute(command);
            return CreatedAtAction(nameof(Get), new { id = booking.Id }, BookingToDto(booking));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}")]
    public IActionResult Get(Guid id)
    {
        var booking = bookings.FindById(id);
        if (booking is null) return NotFound();
        return Ok(BookingToDto(booking));
    }

    private static object BookingToDto(Domain.Entities.Booking b) => new
    {
        b.Id,
        Passengers = b.PassengerDetails.Select(p => new { p.Name, p.Email, p.DocumentNumber }),
        DocumentType = b.DocumentType.ToString(),
        b.Provider,
        b.FlightNumber,
        b.Origin,
        b.Destination,
        b.DepartureUtc,
        b.ArrivalUtc,
        Cabin = b.Cabin.ToString(),
        PassengerCount = b.PassengerDetails.Count,
        b.TotalPrice,
        b.Currency,
        b.BookedAt
    };
}

public sealed record PassengerDto(string Name, string Email, string DocumentNumber);

public sealed record CreateBookingRequest(
    IReadOnlyList<PassengerDto> Passengers,
    DocumentType DocumentType,
    string Provider,
    string FlightNumber,
    string Origin,
    string Destination,
    DateTimeOffset DepartureUtc,
    DateTimeOffset ArrivalUtc,
    CabinClass Cabin,
    decimal TotalPrice);
