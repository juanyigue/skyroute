using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.CreateBooking;
using SkyRoute.Domain.Enums;

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
                request.PassengerName,
                request.DocumentType,
                request.DocumentNumber,
                request.Provider,
                request.FlightNumber,
                request.Origin,
                request.Destination,
                request.DepartureUtc,
                request.ArrivalUtc,
                request.Cabin,
                request.Passengers,
                request.TotalPrice);

            var booking = createBooking.Execute(command);

            return CreatedAtAction(nameof(Get), new { id = booking.Id }, new
            {
                booking.Id,
                booking.PassengerName,
                DocumentType = booking.DocumentType.ToString(),
                booking.DocumentNumber,
                booking.Provider,
                booking.FlightNumber,
                booking.Origin,
                booking.Destination,
                booking.DepartureUtc,
                booking.ArrivalUtc,
                Cabin = booking.Cabin.ToString(),
                booking.Passengers,
                booking.TotalPrice,
                booking.Currency,
                booking.BookedAt
            });
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

        return Ok(new
        {
            booking.Id,
            booking.PassengerName,
            DocumentType = booking.DocumentType.ToString(),
            booking.DocumentNumber,
            booking.Provider,
            booking.FlightNumber,
            booking.Origin,
            booking.Destination,
            booking.DepartureUtc,
            booking.ArrivalUtc,
            Cabin = booking.Cabin.ToString(),
            booking.Passengers,
            booking.TotalPrice,
            booking.Currency,
            booking.BookedAt
        });
    }
}

public sealed record CreateBookingRequest(
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
