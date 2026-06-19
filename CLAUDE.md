# SkyRoute — Claude Context

## Project

Senior full-stack take-home. Angular 21 frontend + .NET 10 backend.

## Folder layout

```
backend/    .NET solution (Domain, Application, Infrastructure, Api, Tests)
frontend/   Angular 21 standalone app
```

## Architecture

Clean architecture, pragmatic layering. Modular monolith — search-aggregation is the named seam for future extraction.

## Backend decisions

- Money: `decimal`, USD only
- Cabin multiplier: Economy ×1 / Business ×2 / First ×3 (applied to base price before provider logic)
- GlobalAir: +15% fuel surcharge, rounded to 2dp (MidpointRounding.AwayFromZero)
- BudgetWings: −10% on base only, $29.99/pax floor; simulates failure via config flag to demo graceful degradation
- Reference data (airports, countries) seeded in-memory behind `IAirportRepository` — source of truth, not a cache
- `IMemoryCache` optional over search results (short TTL only)
- Polly per provider: timeout → retry → circuit breaker
- Document rule: same country = domestic → National ID; different country = international → Passport

## AI strategy

`IAiAssistant.ParseSearch(string text) → SearchQuery`

- `RuleBasedAiAssistant` — deterministic regex/keyword parser, zero config, always works
- `LlmAiAssistant` — Claude API, JSON-schema prompt, low temperature; enabled via `AI__Provider: llm`
- `FallbackAiAssistant` — tries LLM, falls back to rule-based on any error
- Default is rule-based so the reviewer needs no API key

## Frontend decisions

- Standalone components + signals throughout
- Client-side sort via `computed()`
- Dynamic document field driven by domestic/international rule

## Docs

- `ASSUMPTIONS.md` — maintained during build, records every non-obvious decision
- `README.md` — concise; `ASSUMPTIONS.md` content folded in at end
