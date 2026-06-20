using SkyRoute.Domain.Entities;

namespace SkyRoute.Application.Interfaces;

public interface IAirportRepository
{
    Airport? FindByCode(string iataCode);
    IReadOnlyList<Airport> GetAll();
}
