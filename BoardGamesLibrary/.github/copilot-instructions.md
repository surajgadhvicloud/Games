# Project Guidelines

## Code Style
- Use C# with nullable reference types enabled and async-first patterns.
- Keep controllers thin. Put business rules in Infrastructure services.
- Use UTC timestamps for persisted dates (CreatedAtUtc, UpdatedAtUtc, StartDateUtc, EndDateUtc, ReturnDateUtc).
- Prefer explicit, domain-meaningful exceptions:
  - KeyNotFoundException for missing records
  - InvalidOperationException for business rule violations
- Maintain existing naming conventions:
  - Member API naming (Member, IMemberService, MembersController)
  - DTO records named CreateXRequest, UpdateXRequest, XResponse

## Architecture
- Follow the existing layered structure:
  - BoardGamesLibrary.Domain: entities and enums only
  - BoardGamesLibrary.Application: contracts, interfaces, validators
  - BoardGamesLibrary.Infrastructure: DbContext, services, configuration, migrations
  - BoardGamesLibrary.API: controllers, middleware, app startup composition
  - BoardGamesLibrary.Tests: xUnit unit tests
- Keep cross-layer dependencies one-directional:
  - API -> Application + Infrastructure
  - Infrastructure -> Application + Domain
  - Application -> Domain
  - Domain has no dependency on other projects

## Build And Test
- Build all projects:
  - dotnet build BoardGamesLibrary.slnx
- Run tests:
  - dotnet test BoardGamesLibrary.Tests/BoardGamesLibrary.Tests.csproj
- Run API:
  - dotnet run --project BoardGamesLibrary.API/BoardGamesLibrary.API.csproj
- EF Core workflow:
  - dotnet ef migrations add <MigrationName> --project BoardGamesLibrary.Infrastructure --startup-project BoardGamesLibrary.API
  - dotnet ef database update --project BoardGamesLibrary.Infrastructure --startup-project BoardGamesLibrary.API

## Conventions
- Data access and constraints:
  - Configure schema rules in BoardGamesDbContext using Fluent API (indexes, check constraints, FK behavior).
  - Preserve row-version concurrency fields on Inventory and GameIssue.
- Validation:
  - Add FluentValidation validators in Application/Validators.
  - Register validators through Program.cs and keep request models validated before service execution.
- Error handling:
  - Use ExceptionHandlingMiddleware mapping:
    - ValidationException -> 400
    - KeyNotFoundException -> 404
    - InvalidOperationException -> 409
    - fallback -> 500
- Seeding and startup:
  - Startup runs migrate + seed automatically; keep seeding idempotent.
  - Seeder should top up missing records without duplicating existing data.

## Environment Notes
- Current DB target uses SQL Server Express connection string in API appsettings.
- If build fails due to locked DLLs, stop running BoardGamesLibrary.API process and rebuild.
- launchSettings may override ASPNETCORE_URLS during dotnet run.