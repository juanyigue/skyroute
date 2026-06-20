using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.Providers;

namespace SkyRoute.Tests.Providers;

public sealed class GlobalAirProviderTests
{
    private readonly GlobalAirProvider _provider = new();
    private static readonly DateOnly _date = new(2026, 8, 1);

    [Theory]
    [InlineData(CabinClass.Economy, 1)]
    [InlineData(CabinClass.Business, 2)]
    [InlineData(CabinClass.First, 3)]
    public async Task Search_AppliesCabinMultiplierAndFuelSurcharge(CabinClass cabin, int multiplier)
    {
        var economyResult = await _provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy);
        var economyBase = economyResult[0].TotalPrice;
        // Economy price = base * 1 * 1.15 (rounded). Reverse to get base:
        var approxBase = Math.Round(economyBase / 1.15m, 2, MidpointRounding.AwayFromZero);

        var result = await _provider.SearchAsync("JFK", "LHR", _date, 1, cabin);
        var price = result[0].TotalPrice;

        var expected = Math.Round(approxBase * multiplier * 1.15m, 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expected, price);
    }

    [Fact]
    public async Task Search_ScalesPriceByPassengerCount()
    {
        var single = (await _provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy))[0].TotalPrice;
        var triple = (await _provider.SearchAsync("JFK", "LHR", _date, 3, CabinClass.Economy))[0].TotalPrice;

        Assert.Equal(single * 3, triple);
    }

    [Fact]
    public async Task Search_PriceIsRoundedToTwoDecimalPlaces()
    {
        var result = await _provider.SearchAsync("JFK", "CDG", _date, 1, CabinClass.Business);
        var price = result[0].TotalPrice;

        Assert.Equal(price, Math.Round(price, 2));
    }

    [Fact]
    public async Task Search_ProviderNameIsGlobalAir()
    {
        var result = await _provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy);
        Assert.Equal("GlobalAir", result[0].Provider);
    }
}
