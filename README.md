# Subsy

A **subscription management platform** built with **.NET 8** and **Clean Architecture**. Track, analyze, and stay on top of recurring payments through a modern web interface and REST API.

---

## Screenshots

**Landing Page**
![Landing Page](docs/screenshots/firstlook.png)

**Register**
![Register](docs/screenshots/register.png)

**Dashboard — Dark Mode**
![Dashboard Dark](docs/screenshots/dashboard_dark_mode.png)

**Dashboard — Light Mode**
![Dashboard Light](docs/screenshots/dashboard_light_mode.png)

**Active Subscriptions**
![Active Subscriptions](docs/screenshots/active.png)

**Due Today**
![Due Today](docs/screenshots/due.png)

**Calendar**
![Calendar](docs/screenshots/calendar.png)

**Archived Subscriptions**
![Archived](docs/screenshots/archived.png)

**Finance Analytics**
![Finance Analytics](docs/screenshots/Finance_Dashboard.png)

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 8, ASP.NET Core MVC & Web API |
| Architecture | Clean Architecture, CQRS (MediatR) |
| Database | Entity Framework Core + SQLite |
| Auth | ASP.NET Core Identity (Cookie + JWT) |
| Validation | FluentValidation + MediatR pipeline |
| Background Jobs | Hangfire (SQLite storage) |
| Email | MailKit (SMTP) |
| Frontend | Tailwind CSS, Chart.js, FullCalendar |
| Testing | xUnit, Moq, FluentAssertions |
| CI | GitHub Actions |

---

## Architecture

```
Subsy.Web / Subsy.Api
        |
        v
Subsy.Application  (Commands, Queries, DTOs, Validation)
        |
        v
Subsy.Domain       (Entities, Business Rules, Domain Events)
        ^
        |
Subsy.Infrastructure  (EF Core, Identity, Hangfire, Email, Storage)
```

**Dependency rule:** Domain depends on nothing. Application depends only on Domain. Infrastructure and presentation layers point inward.

The Application layer is organized by feature (`Subscriptions/`, `Finance/`, `UserProfile/`, `Admin/`), each with `Commands/` and `Queries/` subfolders following the CQRS pattern.

---

## Features

### Subscription Management
- Create, update, and delete subscriptions with 10 category types
- Archive / unarchive for inactive subscriptions
- Mark as paid with automatic next-payment renewal
- Filter by active, due today, or archived status

### Dashboard
- Active subscription count and today's due items
- Upcoming payments (next 30 days)
- Monthly and yearly spending overview
- Daily average cost calculation

### Finance Analytics
- Monthly and all-time spending breakdowns
- Most expensive subscription identification
- Category distribution (doughnut chart via Chart.js)
- Cost table with daily/monthly/yearly projections
- Smart insights and grouped service cost analysis

### Calendar
- Interactive calendar powered by FullCalendar
- Monthly, weekly, and list views
- AJAX-based dynamic event loading
- Toggle archived subscriptions visibility

### User Profile
- Username and email management
- Password change with Identity validation
- Profile photo upload (local storage or Supabase)
- Two-factor authentication (TOTP with QR code)

### Admin Panel
- User management: assign/revoke admin role, block/unblock, delete
- Subscription overview across all users
- Audit log viewer (domain events trigger automatic logging)
- Broadcast email notifications to all users
- Force logout via security stamp invalidation

### REST API
- JWT Bearer authentication (24-hour token expiry)
- Full subscription CRUD endpoints
- Swagger/OpenAPI documentation
- Independent rate limiting

### Background Jobs
- Daily payment reminder emails (8:00 AM via Hangfire)
- Subscriptions due tomorrow grouped by user
- HTML email templates via MailKit/SMTP

---

## Security

- **Rate limiting:** 60 req/min global, 10/min on login, 30/min on admin area
- **Security headers:** CSP, HSTS (1 year), X-Frame-Options DENY, X-Content-Type-Options nosniff
- **Password policy:** 8+ characters, uppercase + lowercase + digit required
- **Account lockout:** 5 failed attempts triggers 15-minute lockout
- **Email confirmation** required before login

---

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Setup

```bash
# Clone
git clone https://github.com/emirhantopcuoglu/Subsy.git
cd Subsy

# Copy config examples and fill in your values
cp Subsy.Web/appsettings.Development.json.example Subsy.Web/appsettings.Development.json
cp Subsy.Api/appsettings.Development.json.example Subsy.Api/appsettings.Development.json

# Restore and build
dotnet build

# Apply migrations
# (Package Manager Console — Default Project: Subsy.Infrastructure, Startup: Subsy.Web)
Update-Database -Project Subsy.Infrastructure -StartupProject Subsy.Web

# Run the web app
dotnet run --project Subsy.Web

# Or run the API
dotnet run --project Subsy.Api
```

Secret values (email credentials, Supabase keys, JWT signing key) live in the Development config files which are gitignored.

### Running Tests

```bash
dotnet test
```

---

## Project Structure

```
Subsy.sln
├── Subsy.Domain/              Entities, enums, domain events
├── Subsy.Application/         CQRS handlers, DTOs, validation, interfaces
│   ├── Subscriptions/
│   ├── Finance/
│   ├── UserProfile/
│   └── Admin/
├── Subsy.Infrastructure/      EF Core, Identity, Hangfire, email, storage
├── Subsy.Web/                 MVC controllers, views, middleware
│   └── Areas/Admin/           Admin panel controllers & views
├── Subsy.Api/                 REST API controllers, JWT config, Swagger
└── Subsy.Application.Tests/   Unit tests (xUnit + Moq + FluentAssertions)
```

---

## CI/CD

GitHub Actions runs on every push and PR to `main`:
1. Restore dependencies
2. Build (Release configuration)
3. Run test suite

---

## License

This project is for educational and portfolio purposes.
