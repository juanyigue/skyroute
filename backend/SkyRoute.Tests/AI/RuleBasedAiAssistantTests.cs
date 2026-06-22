using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.AI;

namespace SkyRoute.Tests.AI;

public sealed class RuleBasedAiAssistantTests
{
    private readonly RuleBasedAiAssistant _sut = new();

    [Fact]
    public async Task Parse_IataCodes_ExtractsOriginAndDestination()
    {
        var result = await _sut.ParseSearchAsync("JFK to LHR on 2026-08-15");
        Assert.Equal("JFK", result.Origin);
        Assert.Equal("LHR", result.Destination);
    }

    [Fact]
    public async Task Parse_CityNames_MapsToIataCodes()
    {
        var result = await _sut.ParseSearchAsync("New York to London");
        Assert.Equal("JFK", result.Origin);
        Assert.Equal("LHR", result.Destination);
    }

    [Fact]
    public async Task Parse_IsoDate_ExtractsDepartureDate()
    {
        var result = await _sut.ParseSearchAsync("JFK LHR 2026-08-15");
        Assert.Equal(new DateOnly(2026, 8, 15), result.DepartureDate);
    }

    [Fact]
    public async Task Parse_MonthDayYear_ExtractsDepartureDate()
    {
        var result = await _sut.ParseSearchAsync("JFK to LHR on August 15, 2026");
        Assert.Equal(new DateOnly(2026, 8, 15), result.DepartureDate);
    }

    [Fact]
    public async Task Parse_PassengerKeyword_ExtractsPassengers()
    {
        var result = await _sut.ParseSearchAsync("JFK LHR 2 passengers economy");
        Assert.Equal(2, result.Passengers);
    }

    [Fact]
    public async Task Parse_ForNPattern_ExtractsPassengers()
    {
        var result = await _sut.ParseSearchAsync("flight from JFK to LHR for 3");
        Assert.Equal(3, result.Passengers);
    }

    [Theory]
    [InlineData("economy class", CabinClass.Economy)]
    [InlineData("business class", CabinClass.Business)]
    [InlineData("first class", CabinClass.First)]
    [InlineData("business", CabinClass.Business)]
    [InlineData("coach", CabinClass.Economy)]
    public async Task Parse_CabinKeyword_ExtractsCabin(string cabinText, CabinClass expected)
    {
        var result = await _sut.ParseSearchAsync($"JFK LHR {cabinText}");
        Assert.Equal(expected, result.Cabin);
    }

    [Fact]
    public async Task Parse_FullSentence_ExtractsAllFields()
    {
        var result = await _sut.ParseSearchAsync(
            "I need a business class flight from JFK to LHR on 2026-09-01 for 2 passengers");
        Assert.Equal("JFK", result.Origin);
        Assert.Equal("LHR", result.Destination);
        Assert.Equal(new DateOnly(2026, 9, 1), result.DepartureDate);
        Assert.Equal(2, result.Passengers);
        Assert.Equal(CabinClass.Business, result.Cabin);
    }

    [Fact]
    public async Task Parse_UnrecognizedText_ReturnsNulls()
    {
        var result = await _sut.ParseSearchAsync("book me a trip");
        Assert.Null(result.Origin);
        Assert.Null(result.Destination);
        Assert.Null(result.DepartureDate);
        Assert.Null(result.Passengers);
        Assert.Null(result.Cabin);
    }
}
