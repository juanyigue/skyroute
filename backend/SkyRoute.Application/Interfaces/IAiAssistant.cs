using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.Interfaces;

public sealed record ParsedSearchQuery(
    string? Origin,
    string? Destination,
    DateOnly? DepartureDate,
    int? Passengers,
    CabinClass? Cabin);

public interface IAiAssistant
{
    Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default);
}
