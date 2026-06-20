namespace SkyRoute.Domain.Entities;

public sealed class Airport
{
    public string IataCode { get; init; } = default!;
    public string Name { get; init; } = default!;
    public string City { get; init; } = default!;
    public string CountryCode { get; init; } = default!;
}
