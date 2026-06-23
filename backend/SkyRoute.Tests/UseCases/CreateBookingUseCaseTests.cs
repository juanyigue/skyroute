using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.CreateBooking;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.ValueObjects;

namespace SkyRoute.Tests.UseCases;

public sealed class CreateBookingUseCaseTests
{
    private static readonly Airport JFK = new() { IataCode = "JFK", Name = "JFK", City = "New York", CountryCode = "US" };
    private static readonly Airport LAX = new() { IataCode = "LAX", Name = "LAX", City = "Los Angeles", CountryCode = "US" };
    private static readonly Airport LHR = new() { IataCode = "LHR", Name = "Heathrow", City = "London", CountryCode = "GB" };

    private static readonly PassengerInfo Alice = new("Alice Smith", "alice@example.com", "ID123456");

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
        var booking = useCase.Execute(DomesticCommand(DocumentType.NationalId));
        Assert.Equal(DocumentType.NationalId, booking.DocumentType);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void Execute_InternationalRoute_AcceptsPassport()
    {
        var (useCase, _) = Build(JFK, LHR);
        var booking = useCase.Execute(InternationalCommand(DocumentType.Passport));
        Assert.Equal(DocumentType.Passport, booking.DocumentType);
    }

    [Fact]
    public void Execute_DomesticRoute_RejectsPassport()
    {
        var (useCase, _) = Build(JFK, LAX);
        var ex = Assert.Throws<ArgumentException>(() => useCase.Execute(DomesticCommand(DocumentType.Passport)));
        Assert.Contains("NationalId", ex.Message);
    }

    [Fact]
    public void Execute_InternationalRoute_RejectsNationalId()
    {
        var (useCase, _) = Build(JFK, LHR);
        var ex = Assert.Throws<ArgumentException>(() => useCase.Execute(InternationalCommand(DocumentType.NationalId)));
        Assert.Contains("Passport", ex.Message);
    }

    [Fact]
    public void Execute_UnknownOrigin_Throws()
    {
        var (useCase, _) = Build(LAX);
        Assert.Throws<ArgumentException>(() => useCase.Execute(DomesticCommand(DocumentType.NationalId)));
    }

    [Fact]
    public void Execute_EmptyPassengerList_Throws()
    {
        var (useCase, _) = Build(JFK, LAX);
        var cmd = DomesticCommand(DocumentType.NationalId) with { Passengers = [] };
        Assert.Throws<ArgumentException>(() => useCase.Execute(cmd));
    }

    [Fact]
    public void Execute_MultiplePassengers_AllStoredOnBooking()
    {
        var (useCase, _) = Build(JFK, LHR);
        var passengers = new[]
        {
            new PassengerInfo("Alice Smith", "alice@example.com", "P111"),
            new PassengerInfo("Bob Jones", "bob@example.com", "P222"),
        };
        var cmd = InternationalCommand(DocumentType.Passport) with { Passengers = passengers };
        var booking = useCase.Execute(cmd);
        Assert.Equal(2, booking.PassengerDetails.Count);
        Assert.Equal("Alice Smith", booking.PassengerDetails[0].Name);
        Assert.Equal("bob@example.com", booking.PassengerDetails[1].Email);
    }

    [Fact]
    public void Execute_BookingReceivesAllFields()
    {
        var (useCase, _) = Build(JFK, LHR);
        var booking = useCase.Execute(InternationalCommand(DocumentType.Passport));
        Assert.Equal("Alice Smith", booking.PassengerDetails[0].Name);
        Assert.Equal("ID123456", booking.PassengerDetails[0].DocumentNumber);
        Assert.Equal("JFK", booking.Origin);
        Assert.Equal("LHR", booking.Destination);
        Assert.Equal(CabinClass.Business, booking.Cabin);
        Assert.Equal(1200m, booking.TotalPrice);
        Assert.NotEqual(Guid.Empty, booking.Id);
    }

    private static CreateBookingCommand DomesticCommand(DocumentType docType) => new(
        [Alice], docType,
        "GlobalAir", "GA1001", "JFK", "LAX",
        DateTimeOffset.UtcNow.AddDays(30),
        DateTimeOffset.UtcNow.AddDays(30).AddHours(6),
        CabinClass.Business, 1200m);

    private static CreateBookingCommand InternationalCommand(DocumentType docType) => new(
        [Alice], docType,
        "GlobalAir", "GA2001", "JFK", "LHR",
        DateTimeOffset.UtcNow.AddDays(30),
        DateTimeOffset.UtcNow.AddDays(30).AddHours(8),
        CabinClass.Business, 1200m);
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
