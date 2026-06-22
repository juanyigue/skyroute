using SkyRoute.Domain.Entities;

namespace SkyRoute.Application.Interfaces;

public interface IBookingRepository
{
    void Add(Booking booking);
    Booking? FindById(Guid id);
    IReadOnlyList<Booking> GetAll();
}
