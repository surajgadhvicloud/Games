# BoardGamesLibrary

## Local Setup

Run commands from the solution root:

```powershell
cd c:\Code\AIDevelopment\BoardGamesLibrary
```

## Seeder Password Configuration

Seeded auth users (`admin`, `manager`, `dataentry`) now use configuration values instead of hardcoded passwords.

### PowerShell (current terminal session)

```powershell
$env:Seeder__AdminDefaultPassword = "YourStrongAdminPassword!"
$env:Seeder__ManagerDefaultPassword = "YourStrongManagerPassword!"
$env:Seeder__DataEntryDefaultPassword = "YourStrongDataEntryPassword!"
```

### PowerShell (persist for your user profile)

```powershell
setx Seeder__AdminDefaultPassword "YourStrongAdminPassword!"
setx Seeder__ManagerDefaultPassword "YourStrongManagerPassword!"
setx Seeder__DataEntryDefaultPassword "YourStrongDataEntryPassword!"
```

After `setx`, open a new terminal so the values are available.

## JWT Configuration

JWT values are loaded from appsettings and can also be overridden with environment variables if needed.

## One-Time Migration Note (Existing Environments)

If your environment already had seeded users from an older version, existing rows are not overwritten by the seeder.

Do this once when moving to config-driven passwords:

1. Set Seeder environment variables to desired secure values.
2. Apply migrations.
3. Reset or rotate existing seeded user passwords so they match your new policy.

### Apply migration

```powershell
dotnet ef database update --project BoardGamesLibrary.Infrastructure --startup-project BoardGamesLibrary.API
```

### Verify build and tests

```powershell
dotnet build BoardGamesLibrary.slnx
dotnet test BoardGamesLibrary.Tests/BoardGamesLibrary.Tests.csproj
```
