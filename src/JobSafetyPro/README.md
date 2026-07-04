# Job Safety Pro — ASP.NET Core 6 Web API

Clean Architecture solution for enterprise manufacturing safety management.

## Solution Structure

```
JobSafetyPro.sln
src/JobSafetyPro/
├── JobSafetyPro.Domain/          # Entities, enums, repository interfaces, exceptions
├── JobSafetyPro.Application/     # DTOs, services, AutoMapper, FluentValidation
├── JobSafetyPro.Infrastructure/  # EF Core, repositories, JWT, SignalR, audit, seed
└── JobSafetyPro.API/             # Controllers, middleware, Swagger, Serilog
```

## Tech Stack

- ASP.NET Core 6 · EF Core 6 Code First · MSSQL
- JWT + Refresh Tokens · SignalR
- AutoMapper · FluentValidation · Serilog · Swagger

## Run

```bash
# Update connection string in appsettings.Development.json if needed

dotnet ef database update \
  --project src/JobSafetyPro/JobSafetyPro.Infrastructure \
  --startup-project src/JobSafetyPro/JobSafetyPro.API

dotnet run --project src/JobSafetyPro/JobSafetyPro.API
```

Swagger: `https://localhost:7xxx/swagger`

## Demo Credentials

| Email | Password | Role | Primary use |
|-------|----------|------|-------------|
| admin@jsp.demo | Admin@123 | Administrator | Full access, add users |
| hse.manager@jsp.demo | Admin@123 | HSE Manager | Approve JSAs, investigate near misses |
| safety.manager@jsp.demo | Admin@123 | Safety Manager | Capture & manage injuries (portal) |
| safety.officer@jsp.demo | Admin@123 | Safety Officer | Capture & manage injuries (portal) |
| jane.martin@jsp.demo | Employee@123 | Supervisor | Approve JSAs (mobile) |
| john.smith@jsp.demo | Employee@123 | Operator | Submit JSAs, report near misses |

Safety lead accounts are seeded on API startup when missing. Sample operators use `Employee@123`.

## API Endpoints (v1)

| Area | Routes |
|------|--------|
| Auth | `POST /api/v1/auth/login`, `refresh`, `logout`, `GET me` |
| Organization | `/api/v1/companies`, `/plants`, `/departments`, `/users` |
| Safety | `/api/v1/jsas`, `/risk-levels` |
| Incidents | `/api/v1/incidents`, `/near-misses`, `/corrective-actions` |
| Files | `/api/v1/attachments` |
| SignalR | `/hubs/notifications` |

## Architecture Features

- Repository Pattern + Unit of Work
- Soft delete via global query filters + interceptor
- Audit logging (`AuditLogs` table + `IAuditService`)
- `ICurrentUserService` for CreatedBy/ModifiedBy
- Global exception middleware (RFC-style JSON errors)
- Role-based authorization policies

## Database

Code First only — schema changes via EF migrations:

```bash
dotnet ef migrations add <Name> \
  --project src/JobSafetyPro/JobSafetyPro.Infrastructure \
  --startup-project src/JobSafetyPro/JobSafetyPro.API \
  --output-dir Persistence/Migrations
```
