# BoardGamesLibrary

A RESTful Web API for managing a board games lending library — tracking the game catalogue, member accounts, inventory levels, and game issue/return workflows.

---

## Table of Contents

- [Project Overview](#project-overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Architecture](#architecture)
- [Solution Layout](#solution-layout)
- [Domain Model](#domain-model)
- [API Endpoints](#api-endpoints)
- [Pagination](#pagination)
- [Role-Based Access](#role-based-access)
- [Business Rules Configuration](#business-rules-configuration)
- [Local Setup](#local-setup)
- [Seeder Password Configuration](#seeder-password-configuration)
- [JWT Configuration](#jwt-configuration)
- [EF Core Migrations](#ef-core-migrations)
- [Build and Test](#build-and-test)
- [Roadmap](#roadmap)

---

## Project Overview

BoardGamesLibrary is an **ASP.NET Core 8 Web API** that provides a complete back-end for a board games lending library. It supports cataloguing games, tracking physical inventory, registering library members, issuing games to members, monitoring overdue returns, and calculating overdue charges — all secured behind JWT authentication with role-based authorization.

---

## Features

- **Board game catalogue** — create and update game records (name, version, player count, price)
- **Inventory management** — track total vs. available copies; flag missing/broken units
- **Member management** — register Regular and Premium members with contact details
- **Game issue & return workflow** — issue games to members, record return condition, compute overdue charges
- **JWT authentication** — stateless access tokens (configurable expiry) with opaque refresh tokens
- **Refresh token lifecycle** — issue, rotate, and revoke refresh tokens; tokens stored per-user in the database
- **Role-based authorization** — three built-in roles: `Admin`, `Manager`, `DataEntry`
- **Password management** — authenticated password reset endpoint
- **Request validation** — all write operations validated with FluentValidation before hitting the service layer
- **Pagination** — every list endpoint returns a `PagedResponse<T>` with navigation metadata
- **Audit fields** — every entity records `CreatedAtUtc`, `UpdatedAtUtc`, and `ModifiedByUser`
- **Optimistic concurrency** — `Inventory` and `GameIssue` use a `RowVersion` byte array to prevent lost updates
- **Database seeding** — three default auth users (`admin`, `manager`, `dataentry`) seeded from configuration on startup
- **Configurable business rules** — loan period lengths, max active issues per member tier, and overdue daily fee all driven by `appsettings.json`
- **Swagger / OpenAPI UI** — available in development via Swashbuckle
- **Integration tests** — role-access matrix and refresh-token flow tested end-to-end with `WebApplicationFactory` and an in-memory database

---

## Tech Stack

| Concern | Library / Version |
|---|---|
| Runtime | .NET 8 (`net8.0`) |
| Web framework | ASP.NET Core 8 Web API |
| ORM | Entity Framework Core 8 |
| Database | SQL Server (EF Core SqlServer provider) |
| Authentication | `Microsoft.AspNetCore.Authentication.JwtBearer` 8 |
| JWT tokens | `System.IdentityModel.Tokens.Jwt` 8 |
| Password hashing | `Microsoft.Extensions.Identity.Core` 8 |
| Validation | FluentValidation 11 + DI Extensions |
| API docs | Swashbuckle.AspNetCore 6 |
| Unit tests | xUnit 2.5 |
| Integration tests | `Microsoft.AspNetCore.Mvc.Testing` 8 + EF Core InMemory |

---

## Architecture

The solution follows **Clean Architecture** with a strict inward dependency rule:

```
BoardGamesLibrary.API
        │  depends on
        ▼
BoardGamesLibrary.Application   ◄── BoardGamesLibrary.Infrastructure
        │  depends on
        ▼
BoardGamesLibrary.Domain
```

- **Domain** — entities, enums, no external dependencies
- **Application** — service interfaces, request/response contracts, FluentValidation validators
- **Infrastructure** — EF Core `DbContext`, service implementations, JWT logic, seeder, configuration option classes
- **API** — ASP.NET Core controllers, DI composition root, middleware pipeline

---

## Solution Layout

```
BoardGamesLibrary/
├── BoardGamesLibrary.slnx
├── BoardGamesLibrary.API/
│   ├── Controllers/
│   │   ├── AuthController.cs
│   │   ├── BoardGamesController.cs
│   │   ├── GameIssuesController.cs
│   │   ├── InventoriesController.cs
│   │   ├── MembersController.cs
│   │   └── UsersController.cs
│   └── appsettings.json
├── BoardGamesLibrary.Application/
│   ├── Contracts/          # Request / Response records + PagedResponse<T>
│   ├── Interfaces/         # Service interfaces
│   └── Validators/         # FluentValidation validators
├── BoardGamesLibrary.Domain/
│   ├── Entities/           # BoardGame, Member, User, Inventory, GameIssue, RefreshToken
│   └── Enums/              # UserRole, UserType, GameCondition, GameIssueStatus
├── BoardGamesLibrary.Infrastructure/
│   ├── Configuration/      # JwtOptions, SeederOptions, BusinessRulesOptions
│   ├── Data/               # BoardGamesDbContext, DbSeeder
│   ├── Migrations/
│   └── Services/           # AuthService, BoardGameService, MemberService, …
└── BoardGamesLibrary.Tests/
    ├── DbSeederOptionsTests.cs
    ├── GameIssueServiceTests.cs
    ├── MemberInventoryServiceTests.cs
    ├── UserServiceTests.cs
    └── Integration/
        ├── TestWebApplicationFactory.cs
        ├── RefreshTokenFlowIntegrationTests.cs
        └── RoleAccessMatrixIntegrationTests.cs
```

---

## Domain Model

| Entity | Key Fields |
|---|---|
| `BoardGame` | `Id`, `GameName`, `Version`, `MinPlayers`, `MaxPlayers`, `Price` |
| `Member` | `Id`, `FirstName`, `LastName`, `Email`, `PhoneNumber`, `Address`, `TypeOfUser` (`Regular`/`Premium`) |
| `User` | `Id`, `Username`, `Email`, `PasswordHash`, `Role` (`Admin`/`Manager`/`DataEntry`) |
| `Inventory` | `Id`, `BoardGameId` (1-to-1), `TotalInventory`, `AvailableInventory`, `IsMissingOrBroken` |
| `GameIssue` | `Id`, `BoardGameId`, `MemberId`, `StartDateUtc`, `EndDateUtc`, `ReturnDateUtc`, `ConditionGivenOut/In`, `OverdueCharges`, `Status` |
| `RefreshToken` | `Id`, `UserId`, `Token`, `ExpiresAtUtc`, `RevokedAtUtc` |

**Enums**

| Enum | Values |
|---|---|
| `UserRole` | `Admin`, `Manager`, `DataEntry` |
| `UserType` | `Regular`, `Premium` |
| `GameCondition` | `Mint`, `CompleteNotMint`, `Broken`, `Lost` |
| `GameIssueStatus` | `Active`, `Returned`, `Overdue` |

---

## API Endpoints

### Auth — `/api/auth`

| Method | Path | Auth | Description |
|---|---|---|---|
| `POST` | `/api/auth/login` | Anonymous | Obtain JWT + refresh token |
| `POST` | `/api/auth/refresh` | Anonymous | Exchange refresh token for a new JWT |
| `POST` | `/api/auth/revoke` | Any authenticated user | Revoke a refresh token |
| `POST` | `/api/auth/reset-password` | Any authenticated user | Change own password |

### Board Games — `/api/boardgames`

| Method | Path | Min Role | Description |
|---|---|---|---|
| `GET` | `/api/boardgames` | DataEntry | Paginated list |
| `GET` | `/api/boardgames/{id}` | DataEntry | Single record |
| `POST` | `/api/boardgames` | Manager | Create |
| `PUT` | `/api/boardgames/{id}` | Manager | Update |

### Members — `/api/members`

| Method | Path | Min Role | Description |
|---|---|---|---|
| `GET` | `/api/members` | DataEntry | Paginated list |
| `GET` | `/api/members/{id}` | DataEntry | Single record |
| `POST` | `/api/members` | Manager | Create |
| `PUT` | `/api/members/{id}` | Manager | Update |

### Users — `/api/users`

| Method | Path | Min Role | Description |
|---|---|---|---|
| `GET` | `/api/users` | Manager | Paginated list |
| `GET` | `/api/users/{id}` | Manager | Single record |
| `POST` | `/api/users` | Manager | Create |
| `PUT` | `/api/users/{id}` | Manager | Update |

### Inventories — `/api/inventories`

| Method | Path | Min Role | Description |
|---|---|---|---|
| `GET` | `/api/inventories` | DataEntry | Paginated list |
| `GET` | `/api/inventories/{boardGameId}` | DataEntry | Single record by board game |
| `POST` | `/api/inventories` | Manager | Create |
| `PUT` | `/api/inventories/{boardGameId}` | Manager | Update |

### Game Issues — `/api/gameissues`

| Method | Path | Min Role | Description |
|---|---|---|---|
| `GET` | `/api/gameissues` | DataEntry | Paginated list |
| `GET` | `/api/gameissues/{id}` | DataEntry | Single record |
| `POST` | `/api/gameissues` | DataEntry | Issue a game to a member |
| `PUT` | `/api/gameissues/{id}` | DataEntry | Update (e.g. record return) |

---

## Pagination

Every list (`GET` collection) endpoint accepts two optional query parameters and returns a `PagedResponse<T>`:

```
GET /api/boardgames?page=1&pageSize=20
```

**Query parameters**

| Parameter | Default | Description |
|---|---|---|
| `page` | `1` | 1-based page number |
| `pageSize` | `20` | Items per page |

**Response envelope**

```jsonc
{
  "items": [ /* ... */ ],
  "totalCount": 42,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

---

## Role-Based Access

| Role | Permissions |
|---|---|
| **Admin** | Full access to all endpoints |
| **Manager** | Read + write on all resources; create/manage users |
| **DataEntry** | Read-only on board games, members, inventories; full CRUD on game issues |

The three default seeded accounts are `admin`, `manager`, and `dataentry`. Their passwords are set via the [Seeder configuration](#seeder-password-configuration).

---

## Business Rules Configuration

Loan limits and fees are driven by `appsettings.json` (section `BusinessRules`) and can be overridden per environment:

| Key | Default | Meaning |
|---|---|---|
| `PremiumMaxActiveIssues` | `5` | Max concurrent loans for Premium members |
| `RegularMaxActiveIssues` | `2` | Max concurrent loans for Regular members |
| `PremiumLoanDays` | `30` | Loan period (days) for Premium members |
| `RegularLoanDays` | `14` | Loan period (days) for Regular members |
| `OverdueDailyFeeInr` | `250` | Daily overdue charge (₹) |

---

## Local Setup

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server or SQL Server Express (default instance: `localhost\SQLEXPRESS`)
- `dotnet-ef` CLI tool

```powershell
dotnet tool install --global dotnet-ef
```

### Steps

Run commands from the solution root:

```powershell
cd c:\Code\AIDevelopment\BoardGamesLibrary
```

1. Set environment variables for seeder passwords (see [below](#seeder-password-configuration)).
2. Apply migrations to create/update the database.
3. Run the API.

```powershell
dotnet run --project BoardGamesLibrary.API
```

The Swagger UI is available at `https://localhost:{port}/swagger` when running in the `Development` environment.

---

## Seeder Password Configuration

Seeded auth users (`admin`, `manager`, `dataentry`) use configuration values instead of hardcoded passwords. You must supply these values before the first run (and whenever you rotate credentials).

### PowerShell — current terminal session only

```powershell
$env:Seeder__AdminDefaultPassword    = "YourStrongAdminPassword!"
$env:Seeder__ManagerDefaultPassword  = "YourStrongManagerPassword!"
$env:Seeder__DataEntryDefaultPassword = "YourStrongDataEntryPassword!"
```

### PowerShell — persist for your user profile

```powershell
setx Seeder__AdminDefaultPassword    "YourStrongAdminPassword!"
setx Seeder__ManagerDefaultPassword  "YourStrongManagerPassword!"
setx Seeder__DataEntryDefaultPassword "YourStrongDataEntryPassword!"
```

> After `setx`, open a new terminal so the new values are picked up.

### One-time note for existing environments

If your environment already had seeded users from an older version, existing rows are **not** overwritten by the seeder. Do this once when moving to config-driven passwords:

1. Set the Seeder environment variables to your desired secure values.
2. Apply migrations.
3. Manually reset the passwords for the existing seeded users to match your new values.

---

## JWT Configuration

JWT settings live in `appsettings.json` under the `Jwt` section and can be overridden with environment variables (`Jwt__SecretKey`, etc.):

| Key | Default | Description |
|---|---|---|
| `Issuer` | `BoardGamesLibrary` | Token issuer claim |
| `Audience` | `BoardGamesLibraryClient` | Token audience claim |
| `SecretKey` | _(change this)_ | HMAC-SHA256 signing key — **replace in production** |
| `ExpiresInMinutes` | `60` | JWT access token lifetime |
| `RefreshTokenExpiresInDays` | `7` | Refresh token lifetime |

> **Never commit a production `SecretKey` to source control.** Inject it via an environment variable or a secrets manager.

---

## EF Core Migrations

All commands run from the **solution root**.

### Apply pending migrations

```powershell
dotnet ef database update `
  --project BoardGamesLibrary.Infrastructure `
  --startup-project BoardGamesLibrary.API
```

### Add a new migration

```powershell
dotnet ef migrations add <MigrationName> `
  --project BoardGamesLibrary.Infrastructure `
  --startup-project BoardGamesLibrary.API
```

### Current migrations

| Migration | Description |
|---|---|
| `InitialCreate` | Core schema — BoardGame, Member, Inventory, GameIssue |
| `Phase2AuthAndAudit` | User entity, audit columns, role-based auth wiring |
| `RefreshTokenSupport` | RefreshToken entity linked to User |

---

## Build and Test

```powershell
# Restore + build entire solution
dotnet build BoardGamesLibrary.slnx

# Run all tests (unit + integration)
dotnet test BoardGamesLibrary.Tests/BoardGamesLibrary.Tests.csproj
```

Integration tests use `WebApplicationFactory` with an EF Core in-memory database — no real SQL Server required for the test suite.

---

## Roadmap

- [ ] **React-based UI application** — front-end client consuming this API
- [ ] Search and filtering on list endpoints
- [ ] Export reports (overdue issues, inventory summary)
- [ ] Email / notification support for overdue reminders
- [ ] Fine payment tracking and member balance ledger
