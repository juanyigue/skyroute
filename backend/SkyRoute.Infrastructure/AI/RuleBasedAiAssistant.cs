using System.Globalization;
using System.Text.RegularExpressions;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Infrastructure.AI;

public sealed partial class RuleBasedAiAssistant : IAiAssistant
{
    private static readonly HashSet<string> KnownIataCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "JFK", "LAX", "ORD", "MIA", "SFO",
        "LHR", "CDG", "AMS", "FRA",
        "DXB", "SIN", "NRT", "SYD",
        "GRU", "BOG"
    };

    private static readonly Dictionary<string, string> CityToIata = new(StringComparer.OrdinalIgnoreCase)
    {
        ["new york"] = "JFK", ["nyc"] = "JFK",
        ["los angeles"] = "LAX",
        ["chicago"] = "ORD",
        ["miami"] = "MIA",
        ["san francisco"] = "SFO",
        ["london"] = "LHR",
        ["paris"] = "CDG",
        ["amsterdam"] = "AMS",
        ["frankfurt"] = "FRA",
        ["dubai"] = "DXB",
        ["singapore"] = "SIN",
        ["tokyo"] = "NRT",
        ["sydney"] = "SYD",
        ["sao paulo"] = "GRU", ["são paulo"] = "GRU",
        ["bogota"] = "BOG", ["bogotá"] = "BOG",
    };

    public Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default)
    {
        var airports = ExtractAirports(text);
        return Task.FromResult(new ParsedSearchQuery(
            airports.Count > 0 ? airports[0] : null,
            airports.Count > 1 ? airports[1] : null,
            ExtractDate(text),
            ExtractPassengers(text),
            ExtractCabin(text)));
    }

    private static List<string> ExtractAirports(string text)
    {
        var results = new List<string>();

        // Try known IATA codes first (word-boundary match in uppercase)
        var upper = text.ToUpperInvariant();
        foreach (Match m in IataWordPattern().Matches(upper))
        {
            if (KnownIataCodes.Contains(m.Value) && !results.Contains(m.Value))
            {
                results.Add(m.Value);
                if (results.Count == 2) return results;
            }
        }

        if (results.Count >= 2) return results;

        // Supplement with city name lookup (longest names first to avoid "la" matching inside "los angeles")
        foreach (var (city, iata) in CityToIata.OrderByDescending(kv => kv.Key.Length))
        {
            if (text.Contains(city, StringComparison.OrdinalIgnoreCase) && !results.Contains(iata))
            {
                results.Add(iata);
                if (results.Count == 2) break;
            }
        }

        return results;
    }

    private static DateOnly? ExtractDate(string text)
    {
        // ISO date: 2026-08-15
        var iso = IsoDatePattern().Match(text);
        if (iso.Success && DateOnly.TryParse(iso.Value, out var d))
            return d;

        // "August 15" or "Aug 15, 2026"
        var monthDay = MonthDayPattern().Match(text);
        if (monthDay.Success)
        {
            var year = monthDay.Groups[3].Success
                ? int.Parse(monthDay.Groups[3].Value)
                : DateTime.UtcNow.Year;
            if (DateTime.TryParse(
                    $"{monthDay.Groups[1].Value} {monthDay.Groups[2].Value} {year}",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                return DateOnly.FromDateTime(dt);
        }

        return null;
    }

    private static int? ExtractPassengers(string text)
    {
        var m = PassengerCountPattern().Match(text);
        if (m.Success && int.TryParse(m.Groups[1].Value, out var n) && n is >= 1 and <= 9)
            return n;

        var forN = ForNPattern().Match(text);
        if (forN.Success && int.TryParse(forN.Groups[1].Value, out n) && n is >= 1 and <= 9)
            return n;

        return null;
    }

    private static CabinClass? ExtractCabin(string text)
    {
        if (FirstClassPattern().IsMatch(text)) return CabinClass.First;
        if (BusinessPattern().IsMatch(text)) return CabinClass.Business;
        if (EconomyPattern().IsMatch(text)) return CabinClass.Economy;
        return null;
    }

    [GeneratedRegex(@"\b([A-Z]{3})\b")]
    private static partial Regex IataWordPattern();

    [GeneratedRegex(@"\b(\d{4}-\d{2}-\d{2})\b")]
    private static partial Regex IsoDatePattern();

    [GeneratedRegex(@"\b(Jan(?:uary)?|Feb(?:ruary)?|Mar(?:ch)?|Apr(?:il)?|May|Jun(?:e)?|Jul(?:y)?|Aug(?:ust)?|Sep(?:tember)?|Oct(?:ober)?|Nov(?:ember)?|Dec(?:ember)?)\s+(\d{1,2})(?:,?\s*(\d{4}))?", RegexOptions.IgnoreCase)]
    private static partial Regex MonthDayPattern();

    [GeneratedRegex(@"\b(\d+)\s*(?:passenger|pax|person|people|adult)s?", RegexOptions.IgnoreCase)]
    private static partial Regex PassengerCountPattern();

    [GeneratedRegex(@"\bfor\s+(\d+)\b", RegexOptions.IgnoreCase)]
    private static partial Regex ForNPattern();

    [GeneratedRegex(@"\bfirst\s*class\b", RegexOptions.IgnoreCase)]
    private static partial Regex FirstClassPattern();

    [GeneratedRegex(@"\bbusiness(?:\s*class)?\b", RegexOptions.IgnoreCase)]
    private static partial Regex BusinessPattern();

    [GeneratedRegex(@"\beconomy(?:\s*class)?\b|\bcoach\b", RegexOptions.IgnoreCase)]
    private static partial Regex EconomyPattern();
}
