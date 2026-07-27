[![Build](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/build.yml/badge.svg)](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/build.yml)

[![CodeQL](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/github-code-scanning/codeql)

[![Dependabot Updates](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/dependabot/dependabot-updates/badge.svg)](https://github.com/Mulalo-Moditambi/ekasi-property/actions/workflows/dependabot/dependabot-updates)

# Ekasi Property

A property marketplace for the township market — rent out backrooms and cottages,
buy and sell houses ekasi. Built on a pragmatic Clean Architecture template for **.NET 10**
with a React frontend.

## Projects

| Project | What it is |
|---|---|
| `src/Domain` | Entities, value objects, domain events, and error catalogs (DDD, references only `SharedKernel`) |
| `src/Application` | Vertical-slice use cases: MediatR-free command/query handlers, FluentValidation, HybridCache |
| `src/Infrastructure` | EF Core (**SQL Server**, code-first migrations), JWT auth with refresh tokens, permission-based authorization |
| `src/Web.Api` | Minimal API endpoints, rate limiting, OpenTelemetry, ProblemDetails, Swagger |
| `src/Web.Client` | React + TypeScript + Vite frontend (search, listing detail, create/manage listings) |
| `src/SharedKernel` | Common DDD abstractions (`Entity`, `Result`, `Error`, domain events) |

## Domain

The core aggregate is `Property` — a listing with an owner, a township `Address` value object,
a listing type (rent/sale), a property type (backroom, cottage, room, house, flat), pricing,
amenities (electricity, water, own entrance, parking), and a status state machine:

```
Listed ──► Rented ──► (Relist) ──► Listed
Listed ──► Sold                    (terminal)
Listed ──► Withdrawn ──► (Relist) ──► Listed
```

Search (paged, filterable) and listing details are public; creating and managing listings requires
authentication and ownership. Visitors can contact the owner of a listed property without an
account (`Inquiry`), and owners see their inquiries per listing.

## Getting started

```bash
docker compose up -d              # SQL Server + Seq
dotnet run --project src/Web.Api  # API + Swagger on http://localhost:5000

cd src/Web.Client
npm install
npm run dev                       # frontend on http://localhost:5173 (proxies /api to the API)
```

Run the test suite (the integration tests spin up a throwaway SQL Server container, so
Docker must be running):

```bash
dotnet test ekasi-property.slnx
```

## Testing

- `tests/ArchitectureTests` — layer dependency rules
- `tests/Application.UnitTests` — handler + validator tests (in-memory DbContext, NSubstitute)
- `tests/IntegrationTests` — real HTTP against Testcontainers SQL Server

## Credits

Based on the [Clean Architecture template](https://www.milanjovanovic.tech) by Milan Jovanović.
