# SkyRoute

Full-stack flight search and booking application built as a senior take-home exercise.

**Stack:** .NET 10 (ASP.NET Core) · Angular 21 · Vitest · xUnit

---

## Features

- **Flight search** — query two providers (GlobalAir, BudgetWings) simultaneously; results aggregate in real time with graceful degradation if one provider fails
- **Natural-language search** — describe your trip in plain text; the AI assistant parses it and pre-fills the search form (highlighted in green)
- **Client-side sort** — sort results by price, departure time, or duration without any extra API call
- **Multi-passenger booking** — one form per passenger, each with name, email, and document number
- **Dynamic document field** — the required document type (Passport or National ID) is determined by the route (international vs domestic) and applied to every passenger
- **Booking confirmation** — summary page with booking reference and full passenger list

---

## Project structure

```
backend/
  SkyRoute.Domain/          Entity, value objects, enums, domain services
  SkyRoute.Application/     Use cases, repository interfaces
  SkyRoute.Infrastructure/  In-memory repos, flight providers, AI assistants, Polly pipelines
  SkyRoute.Api/             ASP.NET Core MVC controllers, DI wiring
  SkyRoute.Tests/           xUnit tests (domain, use cases, AI parsers)

frontend/
  src/app/
    components/             search-form, results-list (standalone)
    pages/                  search-page, booking-page, confirmation-page (lazy-loaded)
    services/               ApiService, StateService (signals + computed)
    models/                 Shared TypeScript interfaces and constants
```

---

## Running locally

### Backend

```bash
cd backend/SkyRoute.Api
dotnet run
# API available at http://localhost:5094
```

### Frontend

```bash
cd frontend
npm install
ng serve
# App available at http://localhost:4200
```

The Angular dev server proxies `/api` requests to `http://localhost:5094` via `proxy.conf.json`, so both servers must be running.

---

## Running tests

### Backend (xUnit)

```bash
cd backend
dotnet test
```

### Frontend (Vitest via Angular build)

```bash
cd frontend
ng test
```

---

## AI natural-language search

Type a free-text description of your trip into the search box and click **Auto-fill**:

> `JFK to London next Saturday, two passengers, business`

The backend parses it and returns structured fields (origin, destination, date, passengers, cabin). The form is pre-filled and each matched field is highlighted green.

Three AI variants are available, selected via `AI:Provider` in `appsettings.json`:

| Value | Behaviour |
|---|---|
| `rule-based` (default) | Deterministic regex/keyword parser — no API key needed |
| `llm` | Calls Claude Haiku at temperature 0 — requires `AI:Llm:ApiKey` |
| `fallback` | Tries LLM first; falls back to rule-based on any error |

No API key is required to run the application.

---

## Key design decisions

See [ASSUMPTIONS.md](ASSUMPTIONS.md) for the full list of non-obvious decisions made during the build, including:

- Why `RouteClassifier` lives in the Domain layer
- Why sorting is client-side
- Why document type is validated at booking level, not per passenger
- Why HTTPS redirect is disabled in Development
- What passenger fields are collected and why
