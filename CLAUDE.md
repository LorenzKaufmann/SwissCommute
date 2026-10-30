# SwissCommute

Swiss public transport delay monitor with Telegram alerts – .NET 10, Clean Architecture, Azure Functions.

## Purpose of this project

This is a **portfolio project** by Lorenz Kaufmann (C#/.NET developer, ~1.5 years professional experience, BSc Computer Science TU Graz). The goal is to show Swiss employers clean, production-quality engineering: REST integration, Clean Architecture, unit tests, CI/CD and Azure.

Lorenz must be able to explain every design decision in a job interview. Therefore:

- **Explain before implementing.** Briefly describe the approach and *why* (trade-offs, alternatives) before writing code.
- **Small steps.** One focused change at a time, so each step can be reviewed, understood and committed separately.
- **Teach, don't just do.** When introducing a pattern or library (typed HttpClient, Options pattern, DI, etc.), add a short explanation.
- **Suggest commit messages** (Conventional Commits, English) after each completed step.
- Communicate with Lorenz in **German**. Code, comments, commit messages and README are in **English**.

## What the app does

1. A user registers routes they commute on (e.g. Pfäffikon SZ → Zürich HB, departing 07:12).
2. A background job regularly checks these connections via the Swiss public transport API.
3. If a connection is delayed or cancelled, the user gets a Telegram message, ideally with the next alternative connection.

## Architecture

```
src/
  SwissCommute.Core            Domain models and business logic. NO dependencies on other projects or external libraries.
  SwissCommute.Infrastructure  External integrations: transport.opendata.ch client, Telegram client. References Core.
  SwissCommute.Api             ASP.NET Core Web API (endpoints, DI setup, Swagger/OpenAPI). References Core + Infrastructure.
tests/
  SwissCommute.Tests           xUnit tests. References Core + Infrastructure.
```

Planned later: `src/SwissCommute.Functions` – Azure Functions (isolated worker) with a Timer trigger for the periodic delay check.

Rules:
- Dependencies point inward: Core knows nothing about HTTP, Telegram or Azure.
- Core defines interfaces (e.g. `ITransportClient`, `INotificationSender`); Infrastructure implements them.
- Business logic (e.g. "is this connection delayed beyond the threshold?") lives in Core and is fully unit-tested without network access.

## Conventions

- .NET 10, C# latest, nullable reference types enabled, file-scoped namespaces.
- Use `record` types for immutable data/DTOs.
- Typed `HttpClient` via `IHttpClientFactory`; configuration via the Options pattern.
- Async all the way, pass `CancellationToken`.
- Tests: xUnit, Arrange–Act–Assert, descriptive names (`MethodName_Scenario_ExpectedResult`). Mock external APIs; no real network calls in unit tests.
- **Secrets never in the repo.** Telegram bot token etc. via `dotnet user-secrets` locally and app settings / Key Vault in Azure.

## External APIs

**transport.opendata.ch** (free, no API key) – https://transport.opendata.ch/docs.html
- `GET /v1/locations?query=...` – find stations
- `GET /v1/connections?from=...&to=...&date=...&time=...` – connections between two stations
- `GET /v1/stationboard?station=...&limit=...` – departures at a station
- Delay info: `delay` (minutes) on stops / `prognosis` fields. Verify field names against real responses before relying on them.

**Telegram Bot API** – https://core.telegram.org/bots/api
- `POST https://api.telegram.org/bot<token>/sendMessage` with `chat_id` and `text`.

## Constraints

- Development laptop has **8 GB RAM**: avoid Docker unless really needed, keep tooling lightweight.
- **Azure costs must stay ~0 CHF.** Pay-As-You-Go subscription with a 5 CHF budget alert. Use only free-tier-friendly services: Azure Functions (Consumption / Flex Consumption), Storage Account. No SQL Database, Container Instances, VNets, VMs. Region: **Switzerland North**.
- Before creating any Azure resource, state what it costs.

## Common commands

```powershell
dotnet build
dotnet test
dotnet run --project src/SwissCommute.Api
dotnet user-secrets set "Telegram:BotToken" "<token>" --project src/SwissCommute.Api
```

## Weekend plan / status

**Friday (done)**
- [x] Solution structure (Core / Infrastructure / Api / Tests), project references
- [x] GitHub repo (public, MIT license)

**Saturday – foundation & logic**
- [ ] Typed client for transport.opendata.ch in Infrastructure (search stations, get connections)
- [ ] Core domain models (`Connection`, `Route`, `DelayStatus`, ...) and interfaces
- [ ] Delay detection logic in Core ("is my connection delayed? what's the next alternative?") + unit tests
- [ ] API endpoints (manage routes, check a route now) with OpenAPI/Swagger

**Sunday – cloud & polish**
- [ ] Telegram notification sender (Infrastructure) + tests
- [ ] Azure Functions project with Timer trigger that checks routes and sends alerts
- [ ] GitHub Actions: build + test on every push, deploy to Azure
- [ ] README: problem, features, architecture diagram (Mermaid), screenshots, setup guide

**Later ideas**
- Persist routes (Azure Table Storage – cheap)
- Telegram commands to add routes directly from the chat
- Delay statistics per route / simple prediction
