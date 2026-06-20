using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Application.UseCases.SearchFlights;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Tests.UseCases;

public sealed class SearchFlightsUseCaseTests
{
    private static readonly SearchFlightsQuery Query = new(
        "JFK", "LHR", new DateOnly(2026, 8, 1), 1, CabinClass.Economy);

    private static FlightOffer MakeOffer(string provider) => new()
    {
        Provider = provider,
        FlightNumber = "XX001",
        Origin = "JFK",
        Destination = "LHR",
        DepartureUtc = DateTimeOffset.UtcNow,
        ArrivalUtc = DateTimeOffset.UtcNow.AddHours(7),
        Cabin = CabinClass.Economy,
        Passengers = 1,
        TotalPrice = 299m
    };

    [Fact]
    public async Task Execute_ReturnsCombinedOffersFromAllProviders()
    {
        var p1 = Substitute.For<IFlightProvider>();
        p1.ProviderName.Returns("P1");
        p1.SearchAsync(default!, default!, default, default, default, default)
            .ReturnsForAnyArgs([MakeOffer("P1")]);

        var p2 = Substitute.For<IFlightProvider>();
        p2.ProviderName.Returns("P2");
        p2.SearchAsync(default!, default!, default, default, default, default)
            .ReturnsForAnyArgs([MakeOffer("P2")]);

        var useCase = new SearchFlightsUseCase([p1, p2], NullLogger<SearchFlightsUseCase>.Instance);
        var result = await useCase.ExecuteAsync(Query);

        Assert.Equal(2, result.Offers.Count);
        Assert.Empty(result.ProviderErrors);
    }

    [Fact]
    public async Task Execute_ReturnsPartialResultsWhenOneProviderFails()
    {
        var healthy = Substitute.For<IFlightProvider>();
        healthy.ProviderName.Returns("Healthy");
        healthy.SearchAsync(default!, default!, default, default, default, default)
            .ReturnsForAnyArgs([MakeOffer("Healthy")]);

        var failing = Substitute.For<IFlightProvider>();
        failing.ProviderName.Returns("Failing");
        failing.SearchAsync(default!, default!, default, default, default, default)
            .ThrowsAsyncForAnyArgs(new InvalidOperationException("unavailable"));

        var useCase = new SearchFlightsUseCase([healthy, failing], NullLogger<SearchFlightsUseCase>.Instance);
        var result = await useCase.ExecuteAsync(Query);

        Assert.Single(result.Offers);
        Assert.Equal("Healthy", result.Offers[0].Provider);
        Assert.Single(result.ProviderErrors);
        Assert.Contains("Failing", result.ProviderErrors[0]);
    }

    [Fact]
    public async Task Execute_ReturnsAllErrorsWhenAllProvidersFail()
    {
        var p1 = Substitute.For<IFlightProvider>();
        p1.ProviderName.Returns("P1");
        p1.SearchAsync(default!, default!, default, default, default, default)
            .ThrowsAsyncForAnyArgs(new Exception("error1"));

        var p2 = Substitute.For<IFlightProvider>();
        p2.ProviderName.Returns("P2");
        p2.SearchAsync(default!, default!, default, default, default, default)
            .ThrowsAsyncForAnyArgs(new Exception("error2"));

        var useCase = new SearchFlightsUseCase([p1, p2], NullLogger<SearchFlightsUseCase>.Instance);
        var result = await useCase.ExecuteAsync(Query);

        Assert.Empty(result.Offers);
        Assert.Equal(2, result.ProviderErrors.Count);
    }
}
