# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

```powershell
dotnet build                          # Build the project
dotnet run                            # Run (dev: https://localhost:7214)
dotnet watch run                      # Run with hot reload
dotnet ef migrations add <Name>       # Add a new EF Core migration
dotnet ef database update             # Apply pending migrations
```

Development uses LocalDB (`(localdb)\MSSQLLocalDB`), production uses a remote SQL Server. The app auto-applies migrations on startup via `DataSeeder.SeedAsync`.

There are no test projects in this solution.

## Architecture

**ASP.NET Core 8.0 Razor Pages** application — not MVC controllers. All page models live under `Pages/`.

### Request Flow

Browser → Razor Pages (`Pages/`) → Services (`Services/`) → EF Core (`Data/ApplicationDbContext`) → SQL Server

### Key Layers

- **Pages/** — Razor PageModels handle HTTP. Public pages: `Index` (homepage with featured bundles), `Order` (customer order form), `OrderSuccess` (confirmation). Admin pages under `Pages/Admin/` are role-gated with `[Authorize(Roles = "Admin")]`.
- **Services/** — `OrderService` handles order creation with validation (DOB, phone normalization, promo code checks). `GmailEmailSender` sends SMTP email via MailKit. Both registered as scoped DI services.
- **Data/ApplicationDbContext** — Inherits `IdentityDbContext`. DbSets: `Orders`, `Bundles`, `PromoCodes` plus Identity tables. Seed data for bundles is in `OnModelCreating`; admin users are seeded from `appsettings` `Seed:Admins` section at startup.
- **Models/** — `Order`, `Bundle`, `PromoCode`. Order has a custom `[MinAge(18)]` validation attribute. Prices are in BGN (Bulgarian Lev).

### Authentication

ASP.NET Identity with cookie auth. Admin role seeded on startup. No email confirmation required. Identity UI is scaffolded under `Areas/Identity/`.

### Anti-Abuse (Order Form)

The order page enforces a 3-orders-per-month cap per IP address and per DeviceId (GUID stored in `am_device` cookie). A honeypot `TrapField` catches bots. `CustomerKey` is a SHA256 hash of normalized phone+email for future dedup.

### Promo Codes

Stored uppercase, validated case-insensitively. Discount is a percentage applied to bundle price. Validated server-side for existence, active status, and expiry date.

## Language

All user-facing text (form labels, validation messages, enum display names, emails) is in **Bulgarian**. The `DisplayHelper.GetEnumDisplayName()` utility reads `[Display(Name)]` attributes for Bulgarian enum labels.

## Configuration

- **Connection strings**: `appsettings.Development.json` (LocalDB) vs `appsettings.Production.json` (remote SQL Server)
- **Email settings**: `Email` section in appsettings (Gmail SMTP via app password)
- **Admin seeds**: `Seed:Admins` array in appsettings
- **User secrets**: Used for sensitive config (`UserSecretsId` in .csproj)

## Frontend

Bootstrap + jQuery + jQuery Validation (unobtrusive). Static assets in `wwwroot/lib/` managed by LibMan. SCSS compiled via `compilerconfig.json`.
