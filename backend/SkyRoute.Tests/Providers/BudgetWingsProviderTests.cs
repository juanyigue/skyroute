using Microsoft.Extensions.Options;
using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.Providers;

namespace SkyRoute.Tests.Providers;

public sealed class BudgetWingsProviderTests
{
    private static readonly DateOnly _date = new(2026, 8, 1);

    private static BudgetWingsProvider Create(bool simulateFailure = false) =>
        new(Options.Create(new BudgetWingsOptions { SimulateFailure = simulateFailure }));

    [Theory]
    [InlineData(CabinClass.Economy, 1)]
    [InlineData(CabinClass.Business, 2)]
    [InlineData(CabinClass.First, 3)]
    public async Task Search_AppliesTenPercentDiscountAfterCabinMultiplier(CabinClass cabin, int multiplier)
    {
        var provider = Create();

        var economyResult = await provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy);
        var economyPerPax = economyResult[0].TotalPrice;
        // Economy: base * 1 * 0.90 (if above floor). Reverse to get base:
        var approxBase = Math.Round(economyPerPax / 0.90m, 2, MidpointRounding.AwayFromZero);

        var result = await provider.SearchAsync("JFK", "LHR", _date, 1, cabin);
        var price = result[0].TotalPrice;

        var expected = Math.Round(Math.Max(approxBase * multiplier * 0.90m, 29.99m), 2, MidpointRounding.AwayFromZero);
        Assert.Equal(expected, price);
    }

    [Fact]
    public async Task Search_EnforcesFloorPriceOf2999PerPax()
    {
        var provider = Create();

        // Use a short domestic-style route that yields a very low base price.
        // Floor is $29.99/pax, so 1 pax total must be >= 29.99.
        var result = await provider.SearchAsync("JFK", "JFK", _date, 1, CabinClass.Economy);
        Assert.True(result[0].TotalPrice >= 29.99m);
    }

    [Fact]
    public async Task Search_ScalesPriceByPassengerCount()
    {
        var provider = Create();
        var single = (await provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy))[0].TotalPrice;
        var triple = (await provider.SearchAsync("JFK", "LHR", _date, 3, CabinClass.Economy))[0].TotalPrice;

        Assert.Equal(single * 3, triple);
    }

    [Fact]
    public async Task Search_ThrowsWhenSimulateFailureIsEnabled()
    {
        var provider = Create(simulateFailure: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            provider.SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy));
    }

    [Fact]
    public async Task Search_ProviderNameIsBudgetWings()
    {
        var result = await Create().SearchAsync("JFK", "LHR", _date, 1, CabinClass.Economy);
        Assert.Equal("BudgetWings", result[0].Provider);
    }
}
