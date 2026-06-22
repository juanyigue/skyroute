using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Infrastructure.AI;

public sealed class LlmAiAssistant(HttpClient http, IOptions<LlmOptions> options) : IAiAssistant
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public async Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default)
    {
        var body = new
        {
            model = "claude-haiku-4-5-20251001",
            max_tokens = 256,
            temperature = 0,
            messages = new[] { new { role = "user", content = BuildPrompt(text) } }
        };

        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages")
        {
            Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json")
        };
        req.Headers.Add("x-api-key", options.Value.ApiKey);
        req.Headers.Add("anthropic-version", "2023-06-01");

        var resp = await http.SendAsync(req, ct);
        resp.EnsureSuccessStatusCode();

        var json = await resp.Content.ReadAsStringAsync(ct);
        var doc = JsonDocument.Parse(json);
        var content = doc.RootElement.GetProperty("content")[0].GetProperty("text").GetString() ?? "{}";
        return ParseResponse(content);
    }

    private static string BuildPrompt(string text) =>
        $$"""
         Extract flight search parameters from the text below. Respond with JSON only — no prose, no code fences.
         Schema: {"origin":"IATA or null","destination":"IATA or null","departureDate":"YYYY-MM-DD or null","passengers":number or null,"cabin":"Economy|Business|First or null"}
         Known IATA codes: JFK(New York), LAX(Los Angeles), ORD(Chicago), MIA(Miami), SFO(San Francisco), LHR(London), CDG(Paris), AMS(Amsterdam), FRA(Frankfurt), DXB(Dubai), SIN(Singapore), NRT(Tokyo), SYD(Sydney), GRU(Sao Paulo), BOG(Bogota)
         Text: {{text}}
         """;

    private static ParsedSearchQuery ParseResponse(string json)
    {
        try
        {
            // Strip optional markdown code fences the model may emit
            var clean = json.Trim().TrimStart('`');
            if (clean.StartsWith("json", StringComparison.OrdinalIgnoreCase))
                clean = clean[4..];
            clean = clean.TrimEnd('`').Trim();

            var doc = JsonDocument.Parse(clean);
            var root = doc.RootElement;

            var origin = TryString(root, "origin");
            var destination = TryString(root, "destination");

            DateOnly? date = null;
            if (TryString(root, "departureDate") is string ds && DateOnly.TryParse(ds, out var pd))
                date = pd;

            int? passengers = null;
            if (root.TryGetProperty("passengers", out var pax) && pax.ValueKind == JsonValueKind.Number)
                passengers = pax.GetInt32();

            CabinClass? cabin = null;
            if (TryString(root, "cabin") is string cs && Enum.TryParse<CabinClass>(cs, true, out var pc))
                cabin = pc;

            return new ParsedSearchQuery(origin, destination, date, passengers, cabin);
        }
        catch
        {
            return new ParsedSearchQuery(null, null, null, null, null);
        }
    }

    private static string? TryString(JsonElement root, string key)
    {
        if (!root.TryGetProperty(key, out var prop)) return null;
        if (prop.ValueKind == JsonValueKind.Null) return null;
        return prop.GetString();
    }
}
