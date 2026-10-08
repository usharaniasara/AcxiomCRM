# AcxiomCRM

AcxiomCRM is a student-focused ASP.NET Core CRM application built to demonstrate authentication, authorization, CRM workflows, dashboard analytics, reporting, validation, audit logging and a secure REST API in a clean and easy-to-understand MVC structure.

## Problem statement

The project is designed to model a small sales CRM system used by an organization with three role types:

- Admin
- Manager
- SalesExecutive

The application must support customer, lead, opportunity, follow-up and activity management while enforcing role-based permissions and protecting sensitive data.

## Features

- ASP.NET Core Identity authentication and authorization
- Role-based access using Admin, Manager, and SalesExecutive
- Customer management with validation and filtering
- Lead management with status workflow and conversion
- Opportunity pipeline with validation and weighted pipeline value
- Follow-up tracking with planned/completed/missed/cancelled statuses
- Activity tracking for calls, meetings, emails and tasks
- Dashboard and Chart.js KPI views
- Reports and audit log screens
- ASP.NET Core Web API endpoints with DTOs and validation
- Audit trail for create/update/delete/security events
- Seed data for local development
- EF Core SQLite database configuration with future-ready migration support

## Technology stack

- ASP.NET Core 9 MVC
- ASP.NET Core Web API
- ASP.NET Core Identity
- Entity Framework Core + SQLite
- Razor Views
- Bootstrap 5
- Chart.js
- xUnit for tests

## Architecture

The project follows a simple layered structure:

- Controllers: UI and API entry points
- Services: business logic and audit handling
- Models: domain entities
- Data: EF Core DbContext and database seeding
- ViewModels: MVC-specific UI models
- Validators: central validation rules
- Authorization: role/ownership rules
- DTOs: rest API payloads

## Folder structure

- `Controllers/` – MVC controllers
- `Api/` – protected REST API controllers
- `Models/` – CRM and identity entities
- `Services/` – business logic
- `Data/` – EF Core context and seeding
- `ViewModels/` – forms and dashboard models
- `Views/` – Razor UI pages
- `DTOs/` – API request/response models
- `Validators/` – validation helpers and rules
- `Authorization/` – server-side access checks
- `Configuration/` – config objects
- `Constants/` – role names and status lists
- `wwwroot/` – static assets

## Database design

Main entities in the database include:

- Customer
- Lead
- Opportunity
- FollowUp
- Activity
- AuditLog
- ApplicationUser (Identity user)

Relationships are configured using foreign keys and safe delete behavior so that audit/history is not accidentally destroyed.

## Roles

- Admin: full system access
- Manager: team-level visibility and operations
- SalesExecutive: assigned-record access only

## Authorization

The authorization model is enforced on the server using:

- ASP.NET Core Identity roles
- `[Authorize]` attributes
- custom policies such as `AdminOnly`, `AdminOrManager`, and `CrmUser`
- `ResourceAuthorization` checks for ownership and role rules

## Validation

Validation is implemented in both places:

- client-side via Razor/Bootstrap validation attributes
- server-side via model validation and custom validators

Security-critical rules are repeated in business logic services and controllers.

## Security

- password hashing through ASP.NET Identity
- password policy configured in `Program.cs`
- lockout configuration via Identity
- anti-forgery tokens in forms
- protected API endpoints
- role-based permissions
- no passwords, hashes, or tokens exposed in API responses or UI

## API endpoints

Important endpoints include:

- `GET /api/customers`
- `GET /api/customers/{id}`
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `DELETE /api/customers/{id}`
- `GET /api/leads`
- `POST /api/leads`
- `GET /api/opportunities`
- `POST /api/opportunities`

Protected API endpoints require authentication and are role-aware.

## Dashboard

The dashboard presents KPI cards and Chart.js visualizations for:

- lead status
- opportunity pipeline stage distribution
- monthly sales by won amount

The dashboard data is scoped by role and user assignment.

## Reports

The application exposes reports for customer, lead, follow-up, opportunity, pipeline, conversion, user activity and audit logs. Reports include filtering and pagination.

## Audit logging

Audit events are written through an `IAuditService` implementation for important actions such as:

- login success and failure
- logout
- lockout
- create/update/delete/deactivate
- role changes
- lead conversion
- follow-up completion or rescheduling

## Setup instructions

1. Install .NET 9 SDK.
2. Open the project folder in VS Code.
3. Restore dependencies:

   ```bash
   dotnet restore
   ```

4. Apply migrations:

   ```bash
   dotnet ef database update
   ```

5. Run the application:

   ```bash
   dotnet run
   ```

6. Open the browser at the local URL shown by the terminal.

## Database migration instructions

If you need to create a migration after model changes:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

## Seed data

Seed data is generated in `Data/DbInitializer.cs`.

Development users created include:

- Admin: admin@axciomcrm.com / Admin@12345
- Manager: manager@axciomcrm.com / Manager@12345
- SalesExecutive: sales@axciomcrm.com / Sales@12345

These are demo credentials for local use only.

## How to run

```bash
dotnet restore
dotnet build
dotnet run
```

## How to test

```bash
dotnet test
```

## GitHub instructions

```bash
git init
git add .
git commit -m "Initial AcxiomCRM commit"
git branch -M main
git remote add origin <your-github-url>
git push -u origin main
```

## Future enhancements

- CSV and Excel exports
- PDF reports
- email reminders
- profile pages
- advanced dashboard filters
- soft-delete recovery workflow

## How the project works

A typical flow is:

1. User logs in with ASP.NET Identity.
2. The app loads a dashboard based on the user role.
3. The user creates or updates CRM records through controllers and services.
4. The service validates the business rules and writes audit entries.
5. Data is stored in SQLite using EF Core.
6. The API layer exposes selected operations using DTOs and role checks.

This makes the application easy to understand and easy to extend.
