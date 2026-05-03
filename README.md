# Subsy

Subsy is a **Clean Architecture subscription management web application** built with **.NET 8 (ASP.NET Core MVC)**.

It allows users to manage, track, and analyze their recurring subscriptions in a structured, production-oriented backend architecture.

> 🚧 This project is actively under development.

---

## Version

Current Version: **v0.19.0**

* v0.2.0 – Dashboard, Finance analytics, Calendar, User Profile
* v0.2.1 – Global error handling, flash message refactor, production hardening
* v0.3.0 – Cloud object storage (Cloudflare R2) for profile photo uploads

---

## Tech Stack

* **.NET 8** – ASP.NET Core MVC
* **Clean Architecture**
* **MediatR** – CQRS (Commands & Queries)
* **FluentValidation**
* **Entity Framework Core**
* **SQLite**
* **ASP.NET Core Identity**
* **Tailwind CSS**
* **FullCalendar**
* **Chart.js**

---

## Architecture Overview

```
Subsy.Web (MVC)
 ├── Controllers
 ├── ViewModels
 ├── Filters
 ├── ViewComponents
 └── Dependency Injection
        ↓
Subsy.Application
 ├── Commands / Queries
 ├── DTOs
 ├── Validation
 └── Interfaces
        ↓
Subsy.Domain
 ├── Entities
 ├── Business Rules
        ↑
Subsy.Infrastructure
 ├── EF Core
 ├── Identity
 ├── Persistence
 └── Services
```

---

## Dependency Rule

* Web → Application & Infrastructure
* Application → Domain
* Infrastructure → Domain
* Domain → depends on nothing

Business logic lives in the **Domain**.
Frameworks remain implementation details.

---

## Core Features

### Subscription Management

* Create / Update subscription
* Archive / Unarchive
* Permanent delete
* Mark as paid
* Active / Due / Archived filtering

---

### Dashboard

* Active subscription count
* Today due count
* Upcoming payments
* Monthly spending overview

---

### Finance Analytics

* Monthly total spending
* All-time spending (active subscriptions)
* Most expensive subscription
* Grouped service cost analysis
* Pie & Bar charts (Chart.js)

---

### Calendar

* Interactive FullCalendar integration
* Monthly / weekly / list view
* AJAX-based dynamic event loading
* Archived toggle option

---

### User Profile

* Update username & email
* Change password
* Profile photo upload
* Safe file handling
* Identity integration

---

## Validation & Error Handling

* Centralized validation via FluentValidation
* Validation mapped into ModelState
* Global flash message system
* Production-ready error page
* Environment-based exception handling

---

## Security Considerations

* ASP.NET Core Identity
* Cookie configuration
* Authorization via UserId verification
* File upload sanitization
* Old file cleanup on photo change

---

## Database Setup

SQLite is used for development.

### Migration

Using Visual Studio Package Manager Console:

* Default Project: `Subsy.Infrastructure`
* Startup Project: `Subsy.Web`

```powershell
Update-Database -Project Subsy.Infrastructure -StartupProject Subsy.Web
```

---

## Local Configuration

Secret values (email credentials, Supabase keys, JWT signing key) are never committed.
Copy the example files and fill in your own values:

```bash
cp Subsy.Web/appsettings.Development.json.example Subsy.Web/appsettings.Development.json
cp Subsy.Api/appsettings.Development.json.example Subsy.Api/appsettings.Development.json
```

Both files are in `.gitignore` — they will not be committed.

---

## Run

Start:

```
Subsy.Web
```

Navigate to:

```
https://localhost:{port}
```

---

## Roadmap

Planned improvements:

* Background job system (payment reminders)
* Email notifications
* Audit logging
* Unit & integration tests
* CI pipeline
* AI-powered subscription insights

---

## Why Subsy?

This project focuses on:

* Clean Architecture principles
* Separation of concerns
* Maintainability
* Real-world backend structure
* Production-oriented mindset

It is intentionally structured the way modern backend teams design scalable applications.

---

