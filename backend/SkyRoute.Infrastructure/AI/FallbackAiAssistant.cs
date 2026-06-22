using Microsoft.Extensions.Logging;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Infrastructure.AI;

public sealed class FallbackAiAssistant(
    IAiAssistant primary,
    IAiAssistant fallback,
    ILogger<FallbackAiAssistant> logger) : IAiAssistant
{
    public async Task<ParsedSearchQuery> ParseSearchAsync(string text, CancellationToken ct = default)
    {
        try
        {
            return await primary.ParseSearchAsync(text, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Primary AI assistant failed; falling back to rule-based parser");
            return await fallback.ParseSearchAsync(text, ct);
        }
    }
}
