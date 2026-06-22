using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Api.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController(IAiAssistant assistant) : ControllerBase
{
    [HttpPost("parse-search")]
    public async Task<IActionResult> ParseSearch(
        [FromBody] ParseSearchRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
            return BadRequest("Text is required.");

        var result = await assistant.ParseSearchAsync(request.Text, ct);
        return Ok(new
        {
            result.Origin,
            result.Destination,
            DepartureDate = result.DepartureDate?.ToString("yyyy-MM-dd"),
            result.Passengers,
            Cabin = result.Cabin?.ToString()
        });
    }
}

public sealed record ParseSearchRequest(string Text);
