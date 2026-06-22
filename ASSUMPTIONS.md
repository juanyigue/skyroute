# Assumptions

Decisions made during the build that were not explicitly specified in the brief.

---

## RouteClassifier lives in the Domain layer

**Assumption:** The domestic/international detection logic (`RouteClassifier`) is a pure static method in `SkyRoute.Domain.Services`, not an Application service.  
**Reason:** It takes two country codes and returns a `DocumentType` — no I/O, no dependencies. Keeping it in Domain makes it reusable by any use case without injecting a service, and the logic is an intrinsic business rule, not an orchestration concern.

---

## Document type validation is enforced in the use case, not the controller

**Assumption:** `CreateBookingUseCase` resolves both airports from `IAirportRepository`, computes the expected `DocumentType` via `RouteClassifier`, and throws `ArgumentException` if the submitted type doesn't match.  
**Reason:** Controllers should not contain business rules. The use case is the single authoritative place to enforce the domestic/international constraint, regardless of which entry point (HTTP, background job, test) calls it.

---

## IBookingRepository registered as Singleton

**Assumption:** `InMemoryBookingRepository` is registered as a Singleton so bookings survive across HTTP requests during the demo.  
**Reason:** A Scoped registration would create a new (empty) repository per request, making `GET /api/bookings/{id}` always return 404. Singleton keeps the in-memory store alive for the process lifetime, which is the expected demo behaviour.

---

## Booking ID is server-generated

**Assumption:** The `Booking.Id` (Guid) is generated in the entity constructor — the client never sends an ID.  
**Reason:** Client-supplied IDs introduce collision risk and are unnecessary when the server owns the store. The generated ID is returned in the 201 response and used for subsequent GETs.

---

## AI assistant defaults to rule-based; no API key required

**Assumption:** `AI:Provider` defaults to `"rule-based"` in `appsettings.json`. The reviewer can run the full app, including natural-language search, without any API key.  
**Reason:** The brief requires an AI feature but reviewers should not need external credentials to evaluate the submission. The rule-based parser is deterministic and covers all demo scenarios.

---

## Three AI assistant variants behind a single interface

**Assumption:** `IAiAssistant` has three implementations — `RuleBasedAiAssistant`, `LlmAiAssistant`, and `FallbackAiAssistant` — selected via the `AI:Provider` config key (`"rule-based"` / `"llm"` / `"fallback"`).  
**Reason:** This demonstrates the design pattern (strategy + fallback) without forcing the reviewer to configure an API key. Setting `AI:Provider: fallback` shows graceful degradation: the LLM is tried first, and any error silently falls back to the rule-based parser.

---

## LlmAiAssistant uses Claude Haiku

**Assumption:** `LlmAiAssistant` calls `claude-haiku-4-5-20251001` at temperature 0.  
**Reason:** Search-query extraction is a structured extraction task — low temperature maximises determinism. Haiku is significantly faster and cheaper than Sonnet/Opus for this workload, and the task does not require reasoning depth.

---

## RuleBasedAiAssistant only matches known IATA codes

**Assumption:** The regex for IATA detection in `RuleBasedAiAssistant` matches 3-letter uppercase words, then filters against a hardcoded set of the 15 seeded airports.  
**Reason:** Without the allowlist, common English words ("FOR", "THE", "LAX" in a sentence like "the tax is…") would produce false-positive airport codes. Constraining to known codes keeps precision high at the cost of recall for unsupported airports, which is acceptable for a demo scope.

---

## FallbackAiAssistant catches all exceptions

**Assumption:** `FallbackAiAssistant` catches `Exception` (not a specific subset) when the LLM call fails.  
**Reason:** The LLM path can fail in multiple ways — network errors, 401 Unauthorized (bad/missing API key), 429 rate-limit, malformed JSON response. Catching all exceptions ensures the rule-based fallback always activates, which is the correct behaviour for a demo where the API key may not be configured.
