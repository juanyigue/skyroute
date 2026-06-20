using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.UseCases.SearchFlights;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/flights")]
public sealed class FlightsController(SearchFlightsUseCase searchFlights) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string origin,
        [FromQuery] string destination,
        [FromQuery] DateOnly departureDate,
        [FromQuery] int passengers,
        [FromQuery] CabinClass cabin,
        CancellationToken ct)
    {
        if (passengers < 1 || passengers > 9)
            return BadRequest("Passengers must be between 1 and 9.");

        var query = new SearchFlightsQuery(origin, destination, departureDate, passengers, cabin);
        var result = await searchFlights.ExecuteAsync(query, ct);

        return Ok(new
        {
            offers = result.Offers.Select(o => new
            {
                o.Provider,
                o.FlightNumber,
                o.Origin,
                o.Destination,
                o.DepartureUtc,
                o.ArrivalUtc,
                Cabin = o.Cabin.ToString(),
                o.Passengers,
                o.TotalPrice,
                o.Currency
            }),
            errors = result.ProviderErrors
        });
    }
}
