# Games Workspace

This repository currently contains the **BoardGamesLibrary** solution (ASP.NET Core Web API + supporting projects).

## What This Project Is

**BoardGamesLibrary** is a layered .NET 8 solution for managing a board games library:
- A Web API for authentication and CRUD-style operations
- A clean architecture split across API/Application/Infrastructure/Domain
- Tests (unit + integration-style) using xUnit

## Current Features

- **Authentication**
  - Login that returns **JWT access token** + **refresh token**
  - Refresh token flow and token revocation (see API auth endpoints)
- **Users**
  - Create/update users with roles
- **Members**
  - Create/update members (contact details + user type)
- **Board Games**
  - Catalog management (name, version, player counts, price)
- **Inventory**
  - Track total/available inventory and missing/broken flags
  - Optimistic concurrency via `RowVersion` on inventory records
- **Game Issue / Returns**
  - Issue a board game to a user with start/end dates and condition
  - Track returns, condition on return, overdue charges, and status
  - Optimistic concurrency via `RowVersion` on game issues

## Tech Stack

- **Runtime / Framework**: .NET 8, ASP.NET Core
- **Data**: Entity Framework Core 8, SQL Server (configured in API settings)
- **Validation**: FluentValidation
- **Auth**: JWT Bearer auth, refresh tokens
- **API Docs**: Swagger / OpenAPI (Swashbuckle)
- **Testing**: xUnit, `Microsoft.AspNetCore.Mvc.Testing`, EF Core InMemory, coverlet collector

## Solution Layout

- `BoardGamesLibrary/BoardGamesLibrary.API` — HTTP endpoints (thin controllers)
- `BoardGamesLibrary/BoardGamesLibrary.Application` — contracts (DTOs), interfaces, validators
- `BoardGamesLibrary/BoardGamesLibrary.Infrastructure` — EF Core DbContext, services, configuration, DI
- `BoardGamesLibrary/BoardGamesLibrary.Domain` — entities and enums
- `BoardGamesLibrary/BoardGamesLibrary.Tests` — xUnit tests

## Build / Test / Run

Run from the solution root:

```powershell
cd BoardGamesLibrary

# Build
dotnet build BoardGamesLibrary.slnx

# Test
dotnet test BoardGamesLibrary.Tests/BoardGamesLibrary.Tests.csproj

# Run API
dotnet run --project BoardGamesLibrary.API/BoardGamesLibrary.API.csproj
```

## EF Core Migrations

```powershell
cd BoardGamesLibrary

dotnet ef migrations add <MigrationName> --project BoardGamesLibrary.Infrastructure --startup-project BoardGamesLibrary.API
dotnet ef database update --project BoardGamesLibrary.Infrastructure --startup-project BoardGamesLibrary.API
```

## Configuration Notes

### Seeder passwords

Seeded auth users (e.g., `admin`, `manager`, `dataentry`) can be configured via environment variables:

```powershell
$env:Seeder__AdminDefaultPassword = "YourStrongAdminPassword!"
$env:Seeder__ManagerDefaultPassword = "YourStrongManagerPassword!"
$env:Seeder__DataEntryDefaultPassword = "YourStrongDataEntryPassword!"
```

### JWT

JWT values are loaded from `appsettings.json` / `appsettings.Development.json` and can be overridden via environment variables.

## What We’ve Done So Far (High-Level)

- Implemented a clean, layered architecture (API/Application/Infrastructure/Domain)
- Added authentication with JWT + refresh tokens
- Added core domain capabilities: Users, Members, Board Games, Inventory, Game Issues
- Added validation with FluentValidation
- Added automated tests with xUnit

## Roadmap (Next)

Keep this section short and concrete; update as plans change.

- [ ] React-based UI application (login + library management using the existing API)

## Keeping This README Updated

This README is intended to reflect the **current state** of the codebase.

When major changes land (new module, auth changes, DB/migrations changes, project structure changes), update:
- **Current Features**
- **Tech Stack**
- **Solution Layout**
- **Build / Run / Migration** instructions

If you use Copilot, you can optionally create a local custom agent (“README Maintainer”) to help generate and update this file consistently after big changes.
