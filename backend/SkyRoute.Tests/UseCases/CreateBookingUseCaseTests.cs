using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.CreateBooking;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Tests.UseCases;

public sealed class CreateBookingUseCaseTests
{
    private static readonly Airport JFK = new() { IataCode = "JFK", Name = "JFK", City = "New York", CountryCode = "US" };
    private static readonly Airport LAX = new() { IataCode = "LAX", Name = "LAX", City = "Los Angeles", CountryCode = "US" };
    private static readonly Airport LHR = new() { IataCode = "LHR", Name = "Heathrow", City = "London", CountryCode = "GB" };

    private static (CreateBookingUseCase UseCase, IBookingRepository Repo) Build(params Airport[] airports)
    {
        var repo = new FakeBookingRepository();
        var airportRepo = new FakeAirportRepository(airports);
        return (new CreateBookingUseCase(repo, airportRepo), repo);
    }

    [Fact]
    public void Execute_DomesticRoute_AcceptsNationalId()
    {
        var (useCase, repo) = Build(JFK, LAX);
        var cmd = DomesticCommand(DocumentType.NationalId);

        var booking = useCase.Execute(cmd);

        Assert.Equal(DocumentType.NationalId, booking.DocumentType);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Execute_InternationalRoute_AcceptsPassport()
    {
        var (useCase, _) = Build(JFK, LHR);
        var cmd = InternationalCommand(DocumentType.Passport);

        var booking = useCase.Execute(cmd);

        Assert.Equal(DocumentType.Passport, booking.DocumentType);
    }

    [Fact]
    public void Execute_DomesticRoute_RejectsPassport()
    {
        var (useCase, _) = Build(JFK, LAX);
        var cmd = DomesticCommand(DocumentType.Passport);

        var ex = Assert.Throws<ArgumentException>(() => useCase.Execute(cmd));
        Assert.Contains("NationalId", ex.Message);
    }

    [Fact]
    public void Execute_InternationalRoute_RejectsNationalId()
    {
        var (useCase, _) = Build(JFK, LHR);
        var cmd = InternationalCommand(DocumentType.NationalId);

        var ex = Assert.Throws<ArgumentException>(() => useCase.Execute(cmd));
        Assert.Contains("Passport", ex.Message);
    }

    [Fact]
    public void Execute_UnknownOrigin_Throws()
    {
        var (useCase, _) = Build(LAX);
        var cmd = DomesticCommand(DocumentType.NationalId);

        Assert.Throws<ArgumentException>(() => useCase.Execute(cmd));
    }

    [Fact]
    public void Execute_BookingReceivesAllFields()
    {
        var (useCase, _) = Build(JFK, LHR);
        var cmd = InternationalCommand(DocumentType.Passport);

        var booking = useCase.Execute(cmd);

        Assert.Equal("Alice Smith", booking.PassengerName);
        Assert.Equal("P123456", booking.DocumentNumber);
        Assert.Equal("JFK", booking.Origin);
        Assert.Equal("LHR", booking.Destination);
        Assert.Equal(CabinClass.Business, booking.Cabin);
        Assert.Equal(2, booking.Passengers);
        Assert.Equal(1200m, booking.TotalPrice);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    private static CreateBookingCommand DomesticCommand(DocumentType docType) => new(
        "Alice Smith", docType, "P123456",
        "GlobalAir", "GA1001", "JFK", "LAX",
        DateTimeOffset.UtcNow.AddDays(30),
        DateTimeOffset.UtcNow.AddDays(30).AddHours(6),
        CabinClass.Business, 2, 1200m);

    private static CreateBookingCommand InternationalCommand(DocumentType docType) => new(
        "Alice Smith", docType, "P123456",
        "GlobalAir", "GA2001", "JFK", "LHR",
        DateTimeOffset.UtcNow.AddDays(30),
        DateTimeOffset.UtcNow.AddDays(30).AddHours(8),
        CabinClass.Business, 2, 1200m);
}

file sealed class FakeBookingRepository : IBookingRepository
{
    private readonly List<Booking> _store = [];
    public void Add(Booking booking) => _store.Add(booking);
    public Booking? FindById(Guid id) => _store.FirstOrDefault(b => b.Id == id);
    public IReadOnlyList<Booking> GetAll() => _store.AsReadOnly();
}

file sealed class FakeAirportRepository(Airport[] airports) : IAirportRepository
{
    public Airport? FindByCode(string iataCode) =>
        airports.FirstOrDefault(a => a.IataCode.Equals(iataCode, StringComparison.OrdinalIgnoreCase));
    public IReadOnlyList<Airport> GetAll() => airports;
}
