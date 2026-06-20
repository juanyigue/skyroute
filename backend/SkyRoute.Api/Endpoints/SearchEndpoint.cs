using SkyRoute.Application.UseCases.SearchFlights;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Api.Endpoints;

public static class SearchEndpoint
{
    public static void MapSearchEndpoints(this WebApplication app)
    {
        app.MapGet("/api/flights/search", HandleAsync)
            .WithName("SearchFlights");
    }

    private static async Task<IResult> HandleAsync(
        string origin,
        string destination,
        DateOnly departureDate,
        int passengers,
        CabinClass cabin,
        SearchFlightsUseCase useCase,
        CancellationToken ct)
    {
        if (passengers < 1 || passengers > 9)
            return Results.BadRequest("Passengers must be between 1 and 9.");

        var query = new SearchFlightsQuery(origin, destination, departureDate, passengers, cabin);
        var result = await useCase.ExecuteAsync(query, ct);

        return Results.Ok(new
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
