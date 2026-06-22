using Microsoft.Extensions.Logging.Abstractions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.AI;

namespace SkyRoute.Tests.AI;

public sealed class FallbackAiAssistantTests
{
    private static FallbackAiAssistant Build(IAiAssistant primary, IAiAssistant? fallback = null) =>
        new(primary, fallback ?? new RuleBasedAiAssistant(), NullLogger<FallbackAiAssistant>.Instance);

    [Fact]
    public async Task ParseSearch_WhenPrimarySucceeds_ReturnsPrimaryResult()
    {
        var expected = new ParsedSearchQuery("JFK", "LHR", new DateOnly(2026, 9, 1), 2, CabinClass.Business);
        var sut = Build(new StubAssistant(expected));

        var result = await sut.ParseSearchAsync("anything");

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ParseSearch_WhenPrimaryThrows_FallsBackToRuleBased()
    {
        var sut = Build(new ThrowingAssistant());

        var result = await sut.ParseSearchAsync("JFK to LHR 2026-09-01 business 2 passengers");

        Assert.Equal("JFK", result.Origin);
        Assert.Equal("LHR", result.Destination);
        Assert.Equal(new DateOnly(2026, 9, 1), result.DepartureDate);
        Assert.Equal(CabinClass.Business, result.Cabin);
    }

    [Fact]
    public async Task ParseSearch_WhenPrimaryThrows_ExceptionDoesNotPropagate()
    {
        var sut = Build(new ThrowingAssistant());

        var ex = await Record.ExceptionAsync(() => sut.ParseSearchAsync("JFK LHR"));

        Assert.Null(ex);
    }

    [Fact]
    public async Task ParseSearch_WhenPrimaryThrowsOnUnrecognizedText_FallbackReturnsNulls()
    {
        var sut = Build(new ThrowingAssistant());

        var result = await sut.ParseSearchAsync("book me a trip");

        Assert.Null(result.Origin);
        Assert.Null(result.Destination);
    }
}

file sealed class StubAssistant(ParsedSearchQuery response) : IAiAssistant
{
    public Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default)
        => Task.FromResult(response);
}

file sealed class ThrowingAssistant : IAiAssistant
{
    public Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default)
        => throw new HttpRequestException("Simulated LLM failure");
}
