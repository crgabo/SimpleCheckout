# SimpleCheckout

Lightweight checkout calculation service built with ASP.NET Core Minimal API (.NET 8).

## Getting started

**Prerequisites:** .NET 8 SDK

```bash
# 1. Create the local secrets file
cp appsettings.Development.json.example appsettings.Development.json

# 2. Run
dotnet run
```

The API starts at `http://localhost:5287`. Swagger UI is available at `/swagger`.

The SQLite database (`checkout.db`) is created automatically on first run — no migrations needed.

## Live Demo

**API running at:** https://simplecheckout-production.up.railway.app

**Quick test:**
```bash
# 1. Get auth token
curl -X POST https://simplecheckout-production.up.railway.app/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin"}'

# 2. Create a checkout order (replace TOKEN with the value from step 1)
curl -X POST https://simplecheckout-production.up.railway.app/checkout \
  -H "Authorization: Bearer TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"items":[{"name":"Product","unitPrice":50,"quantity":3}]}'
```

For Swagger UI and interactive testing, run locally: `dotnet run` then visit `http://localhost:5287/swagger`

## Authentication

All `/checkout` endpoints require a JWT Bearer token. Obtain one via the login endpoint:

```
POST /auth/login
{ "username": "admin", "password": "admin" }
```

Use the returned token as `Authorization: Bearer <token>` on subsequent requests, or click **Authorize** in Swagger UI.

## Endpoints

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| `POST` | `/auth/login` | No | Returns a JWT token |
| `POST` | `/checkout` | Bearer | Creates a checkout order |
| `GET`  | `/checkout/{id}` | Bearer | Retrieves an order by ID |

### POST /checkout — request

```json
{
  "items": [
    { "name": "Product A", "unitPrice": 50.00, "quantity": 3 }
  ]
}
```

### POST /checkout — response

```json
{
  "id": "3fa85f64-...",
  "subtotal": 150.00,
  "discount": 15.00,
  "taxes":    17.55,
  "total":    152.55
}
```

**Calculation rules** (loaded from database, configurable):
- `subtotal` = sum of `unit_price × quantity`
- `discount` = 10% of subtotal when subtotal > 100
- `taxes`    = 13% of `subtotal - discount`
- `total`    = `subtotal + taxes - discount`

**Tax calculation note:** Taxes are applied to the discounted subtotal (`subtotal - discount`) rather than the original subtotal. This approach aligns with regional tax standards in Latin America, where value-added tax and other consumption taxes are calculated on the final transaction amount after promotional discounts have been applied.

## Architecture

```
Domain/          → Business entities, repository interfaces, and domain services (no framework dependencies)
Application/     → CheckoutService, validators, JWT token service
  Rules/         → IDiscountCalculator / ITaxCalculator implementations
  Validators/    → IValidator<T> implementations
Infrastructure/  → EF Core DbContext, SQLite repositories
API/             → Minimal API endpoint groups
```

Dependencies flow inward: `API` / `Infrastructure` → `Application` → `Domain`.

### Pricing Calculation

The `IPricingCalculator` domain service orchestrates the pricing logic:
- Accepts a list of `PricingRule` records from the database
- Evaluates discount eligibility and applies matching calculators
- Evaluates tax eligibility and applies matching calculators (with taxes computed on the discounted subtotal)
- Returns a tuple of `(Discount, Taxes)` for total calculation

This separation keeps `CheckoutService` focused on orchestration while pricing logic remains independently testable and configurable.

## Extending

**New discount/tax calculator** (different algorithm):
1. Add a value to `CalculationType` enum (`Domain/PricingRule.cs`)
2. Create a class in `Application/Rules/` implementing `IDiscountCalculator` or `ITaxCalculator`
3. Register it in `Program.cs`
4. Add a record in the `PricingRules` table

**Custom pricing logic** (if you need to change the overall calculation order or strategy):
1. Create a new class implementing `IPricingCalculator` in `Domain/`
2. Register it in `Program.cs`

The default `PricingCalculator` applies discounts first, then taxes on the discounted amount — to change this behavior, create a custom implementation of `IPricingCalculator`.

**New request validator:**
1. Create a class in `Application/Validators/` implementing `IValidator<T>`
2. Register it in `Program.cs`

## Persistence

SQLite is used for local development and demonstration. To switch to PostgreSQL:

1. Replace the NuGet package:
   ```bash
   dotnet remove package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.*
   ```
2. In `Program.cs`, replace `UseSqlite(...)` with `UseNpgsql(...)`
3. Update the connection string in `appsettings.json`

No changes to domain, application, or repository code are required.
