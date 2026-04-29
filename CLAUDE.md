# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build
dotnet build --configuration Release

# Run
dotnet run --project Subsy.Web          # MVC web app
dotnet run --project Subsy.Api          # REST API

# Test
dotnet test
dotnet test --filter "FullyQualifiedName~ClassName"  # single test class

# EF Migrations (run from solution root in Package Manager Console)
Add-Migration MigrationName -Project Subsy.Infrastructure -StartupProject Subsy.Web
Update-Database -Project Subsy.Infrastructure -StartupProject Subsy.Web
```

CI runs on push/PR to `main` via `.github/workflows/ci.yml`.

## Architecture

**Clean Architecture** with strict dependency flow:

```
Subsy.Web / Subsy.Api  →  Subsy.Application  →  Subsy.Domain
Subsy.Infrastructure   →  Subsy.Application  →  Subsy.Domain
```

### Layer responsibilities

| Project | Role |
|---|---|
| `Subsy.Domain` | Entities with business logic, no external deps |
| `Subsy.Application` | CQRS via MediatR, FluentValidation, DTOs |
| `Subsy.Infrastructure` | EF Core + SQLite, Identity, Hangfire, email, exchange rates |
| `Subsy.Web` | ASP.NET Core MVC (cookie auth, Tailwind, Chart.js) |
| `Subsy.Api` | ASP.NET Core Web API (JWT auth, Swagger) |
| `Subsy.Application.Tests` | xUnit + Moq + FluentAssertions |

### CQRS pattern

Application layer is organized by feature (`Subscriptions/`, `Finance/`, `UserProfile/`), each with `Commands/` and `Queries/` subfolders. Handlers use MediatR. All commands/queries are `record` types. `ValidationBehavior` runs FluentValidation as a MediatR pipeline behavior.

### Domain entities

`Subscription` is a rich domain entity with private setters, a `Create` factory method, and business methods (`MarkAsPaid`, `Archive`, `Unarchive`, `UpdateDetails`). Domain events (MediatR notifications) trigger audit log creation automatically.

### Database

SQLite in both development and production. The `ApplicationDbContext` inherits `IdentityDbContext`. Migrations live in `Subsy.Infrastructure/Migrations/`.

### Authentication

- **Web**: ASP.NET Core Identity with cookie auth. Password: 8+ chars, upper + lower + digit. Lockout: 5 failures → 15 min.
- **API**: JWT Bearer tokens. Secrets configured via user-secrets.

### Background jobs

Hangfire with SQLite storage. `PaymentReminderJob` runs daily at 8 AM — finds subscriptions due tomorrow, groups by user, sends email via SMTP.

### Security

`SecurityHeadersMiddleware` sets CSP/HSTS/X-Frame-Options headers. Rate limiting: 60 req/min globally, 10/min on the login endpoint.

## Key conventions

- File-scoped namespaces, nullable reference types enabled, implicit usings enabled.
- Implementations are `sealed`; register DI in each layer's `ServiceCollectionExtensions`.
- `IDateTimeProvider` abstraction wraps `DateTime.UtcNow` for testability.
- Controllers get the current user via `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
- `TempData` carries flash messages across redirects in the MVC app.
- `SubscriptionDto` lives in `Subsy.Application/Subscriptions/Queries/Common/`.

## Git workflow

For every task:
1. Create a feature branch (`feat/`, `fix/`, `chore/`, etc.).
2. Commit in English, following the existing commit style (conventional commits: `feat(scope): description`).
3. After each meaningful step, bump the version in the project's existing versioning file and add a note to the existing changelog. Do NOT create new versioning or changelog files if they don't already exist.
4. After pushing the branch, create and push an annotated version tag and follow Semantic Versioning (SemVer): `MAJOR.MINOR.PATCH` — `fix/` branches increment PATCH, `feat/` branches increment MINOR, breaking changes increment MAJOR. Tag message format:
   ```
   git tag -a v<version> -m "$(cat <<'EOF'
   v<version>

   <Category>:
   - <change description>
   - <change description>
   EOF
   )"
   git push origin v<version>
   ```
5. Never open a pull request via CLI — pushing the branch is enough.
6. Never mention AI tools in commit messages, PR descriptions, or code comments.

## Code quality

- Prioritise clean, safe, and performant code.
- Add comments only when the **why** is non-obvious (hidden constraint, subtle invariant, workaround). Never describe what the code does — names do that.
