## Commands

```bash
# Run locally (HTTP: 5093, HTTPS: 7110)
cd SchoolManagementSystem.Web/SchoolManagementSystem.Web && dotnet run

# Run tests
dotnet test SchoolManagementSystem.Web/SchoolManagementSystem.Tests/

# Docker (production, port 8080)
docker-compose up --build

# Add EF migration
cd SchoolManagementSystem.Web/SchoolManagementSystem.Web
dotnet ef migrations add <Name>
```

## Architecture

Three .NET 9 projects in one solution:
- `SchoolManagementSystem.Web` — Blazor Server host, all business logic, EF Core, Identity
- `SchoolManagementSystem.Web.Client` — Blazor WASM library (TimetableGrid runs client-side)
- `SchoolManagementSystem.Tests` — xUnit tests using EF InMemory + Moq

SQLite database (`school.db`). In Docker, stored at `db/school.db` via volume mount.

Two `.sln` files exist — use the one at the repo root (`SchoolManagementSystem.sln`).

## Key Files

- `SchoolManagementSystem.Web/Program.cs` — service registration, auth config, DB initialization
- `Data/SchoolDbContext.cs` — EF Core context (extends `IdentityDbContext`)
- `Data/DbSeeder.cs` — seeds roles and default admin
- `Services/` — all business logic behind interfaces (`IClassService`, etc.)
- `Components/Pages/` — Blazor pages
- `Controllers/ScheduleController.cs` — REST API for schedule management

## Non-Obvious Patterns

All services extend `BaseService<T>` which wraps calls in `ExecuteSafeAsync()`. Get operations swallow exceptions and return null/empty; write operations catch, log, then rethrow — callers of write operations must handle exceptions.

New user registrations are inactive by default and require admin approval before they can log in (`Components/Pages/Admin/Approvals`).

`ScheduleController` returns HTTP 409 on conflicts (teacher, class, or room double-booked). Conflict detection is multi-level and runs inside a transaction.

## Testing

Tests use EF InMemory, not SQLite — SQLite-specific behaviors won't be caught. To run a specific class: `dotnet test --filter "ClassName=ClassServiceTests"`.

## Default Credentials (seeded)

Admin: `admin@school.com` / `Admin123!`

Password policy: min 8 chars, requires uppercase, lowercase, digit, and special character. Lockout: 5 failed attempts triggers a 15-minute lockout.
