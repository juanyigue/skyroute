using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;

namespace SkyRoute.Infrastructure.Persistence;

public sealed class InMemoryAirportRepository : IAirportRepository
{
    private static readonly IReadOnlyList<Airport> _airports =
    [
        new() { IataCode = "JFK", Name = "John F. Kennedy International", City = "New York", CountryCode = "US" },
        new() { IataCode = "LAX", Name = "Los Angeles International", City = "Los Angeles", CountryCode = "US" },
        new() { IataCode = "ORD", Name = "O'Hare International", City = "Chicago", CountryCode = "US" },
        new() { IataCode = "MIA", Name = "Miami International", City = "Miami", CountryCode = "US" },
        new() { IataCode = "SFO", Name = "San Francisco International", City = "San Francisco", CountryCode = "US" },
        new() { IataCode = "LHR", Name = "Heathrow", City = "London", CountryCode = "GB" },
        new() { IataCode = "CDG", Name = "Charles de Gaulle", City = "Paris", CountryCode = "FR" },
        new() { IataCode = "AMS", Name = "Amsterdam Schiphol", City = "Amsterdam", CountryCode = "NL" },
        new() { IataCode = "FRA", Name = "Frankfurt Airport", City = "Frankfurt", CountryCode = "DE" },
        new() { IataCode = "MAD", Name = "Adolfo Suárez Madrid–Barajas", City = "Madrid", CountryCode = "ES" },
        new() { IataCode = "NRT", Name = "Narita International", City = "Tokyo", CountryCode = "JP" },
        new() { IataCode = "SYD", Name = "Sydney Kingsford Smith", City = "Sydney", CountryCode = "AU" },
        new() { IataCode = "GRU", Name = "São Paulo Guarulhos", City = "São Paulo", CountryCode = "BR" },
        new() { IataCode = "EZE", Name = "Ministro Pistarini", City = "Buenos Aires", CountryCode = "AR" },
        new() { IataCode = "BOG", Name = "El Dorado International", City = "Bogotá", CountryCode = "CO" },
    ];

    private static readonly Dictionary<string, Airport> _byCode =
        _airports.ToDictionary(a => a.IataCode, StringComparer.OrdinalIgnoreCase);

    public Airport? FindByCode(string iataCode) =>
        _byCode.TryGetValue(iataCode, out var airport) ? airport : null;

    public IReadOnlyList<Airport> GetAll() => _airports;
}
