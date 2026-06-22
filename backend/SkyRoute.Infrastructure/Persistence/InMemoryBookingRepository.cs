using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Entities;

namespace SkyRoute.Infrastructure.Persistence;

public sealed class InMemoryBookingRepository : IBookingRepository
{
    private readonly List<Booking> _bookings = [];

    public void Add(Booking booking) => _bookings.Add(booking);
    public Booking? FindById(Guid id) => _bookings.FirstOrDefault(b => b.Id == id);
    public IReadOnlyList<Booking> GetAll() => _bookings.AsReadOnly();
}
