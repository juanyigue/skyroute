using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.UseCases.SearchFlights;

public sealed record SearchFlightsQuery(
    string Origin,
    string Destination,
    DateOnly DepartureDate,
    int Passengers,
    CabinClass Cabin);
