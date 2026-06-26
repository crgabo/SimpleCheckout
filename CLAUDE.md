# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet run                 # build + run (default 'http' profile, http://localhost:5287)
dotnet run --launch-profile https   # also binds https://localhost:7229
dotnet build               # compile only
dotnet watch run           # hot-reload during development
dotnet restore             # restore NuGet packages
```

There is no test project yet. When adding tests, create a separate `xunit` project — run a single test with `dotnet test --filter "FullyQualifiedName~TestName"`.

## Architecture

Layered architecture — dependencies flow inward: `API` / `Infrastructure` → `Application` → `Domain`.

### Domain/
Pure business objects, no framework dependencies.

- `CheckoutOrder` — aggregate with `Subtotal` (computed from items), plus `Taxes`, `Discount`, `Total` as stored `init` properties (set by the service at creation time).
- `OrderItem` — `Name`, `Price`, `Quantity` (all `decimal`).
- `ICheckoutRepository` — persistence abstraction for orders.
- `PricingRule` — data entity: `Id`, `Name`, `Type` (`PricingRuleType`: Tax/Discount), `Calculation` (`CalculationType`: Percentage), `Rate`, `MinSubtotal?`.
- `IPricingRuleRepository` — persistence abstraction for pricing rules.
- `IDiscountCalculator` / `ITaxCalculator` — strategy interfaces. Each declares `Type`, `Applies(rule, items, subtotal)`, and `Apply(rule, items, subtotal)`. The `Applies` method owns the eligibility check; `Apply` owns the math.

### Application/
- `CheckoutService` — orchestrates order creation: runs validators first (throws `ValidationException` on failure), fetches rules from `IPricingRuleRepository`, runs all matching calculators that pass `Applies`, calculates `discount` first then `taxes` on `subtotal - discount`.
- `CheckoutRequest` / `CheckoutResponse` — the only types that cross the HTTP boundary.
- `IValidator<T>` — generic validation interface with `Validate(T) → IReadOnlyList<string>`. Implemented per request type; all registered validators are run by `CheckoutService` before processing.
- `ValidationException` — thrown by the service when validation fails; carries `Errors`. The endpoint catches it and returns `400 ValidationProblem`.
- `Rules/` — concrete calculator implementations. Each implements `IDiscountCalculator` or `ITaxCalculator`:
  - `PercentageDiscountCalculator` — applies when `subtotal > MinSubtotal` (MinSubtotal must be set).
  - `PercentageTaxCalculator` — always applies (`Applies` returns `true`).
- `Validators/` — concrete validator implementations:
  - `CheckoutRequestValidator` — items not empty; per item: name required, unit_price > 0, quantity > 0.
- `TokenService` — generates JWT from fake credentials (`admin`/`admin`). Contains `LoginRequest` and `LoginResponse`.

### Infrastructure/
- `SimpleCheckoutDbContext` — EF Core DbContext. `CheckoutOrder.Subtotal` is ignored (computed); `OrderItem` is an owned entity with a shadow `int` PK; `PricingRule` is seeded with VAT (13%) and bulk discount (10% over 100) via `HasData` with stable GUIDs.
- `EfCheckoutRepository` — implements `ICheckoutRepository` using EF Core + SQLite.
- `EfPricingRuleRepository` — implements `IPricingRuleRepository` using EF Core + SQLite.
- `InMemoryCheckoutRepository` / `InMemoryPricingRuleRepository` — kept unregistered; useful for tests or local reference.

### API/
Static extension classes with `Map*` methods on `WebApplication`:
- `CheckoutEndpoints` — `/checkout` group, requires authorization.
- `AuthEndpoints` — `POST /auth/login`, anonymous.

### Program.cs
Wires DI, configures JWT Bearer auth, Swagger with Bearer support, global exception handler, and calls `Map*` extension methods. Calls `Database.EnsureCreated()` on startup — creates `checkout.db` automatically if it doesn't exist. Intentionally thin.

**To switch to PostgreSQL:** replace `Microsoft.EntityFrameworkCore.Sqlite` with `Npgsql.EntityFrameworkCore.PostgreSQL` and change `UseSqlite(...)` to `UseNpgsql(...)` — no other code changes needed.

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/auth/login` | No | Returns `{ token }` for credentials `admin`/`admin` |
| `POST` | `/checkout` | Bearer | Creates order, returns `201` with calculated fields |
| `GET`  | `/checkout/{id}` | Bearer | Returns order or `404` |

## Error handling

All unhandled exceptions go through `UseExceptionHandler()` + `AddProblemDetails()`:
- `BadHttpRequestException` (malformed JSON) → `400` with detail message
- Any other unhandled exception → `500` without internal details

`ValidationException` is caught per-endpoint and mapped to `400 ValidationProblem`.

## Config

- `appsettings.json` — `Jwt:Issuer`, `Jwt:Audience`, `ConnectionStrings:DefaultConnection`
- `appsettings.Development.json` — `Jwt:Secret`

## Conventions

- Endpoint groups go in `API/` as static classes with `Map*` extension methods. No MVC controllers.
- New calculator types go in `Application/Rules/`, implement `IDiscountCalculator` or `ITaxCalculator`, and are registered in `Program.cs`. No changes needed to `CheckoutService`.
- To add a new `CalculationType`: add the enum value, create the class in `Rules/`, register it.
- New validators go in `Application/Validators/`, implement `IValidator<T>`, and are registered in `Program.cs`. Validation runs in the service, not in the endpoint — the endpoint only catches `ValidationException` and maps it to `400`.
- Domain types must not reference Application, Infrastructure, or ASP.NET types.
- `ImplicitUsings` and nullable reference types are both enabled.
- `SimpleCheckout.http` holds IDE-runnable HTTP examples — add new requests there when adding endpoints.
