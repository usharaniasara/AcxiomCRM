# AcxiomCRM Project Guide

This guide explains the project in plain language so it is easy to understand and discuss in an interview.

## 1. Customer Creation

What is it?
- Creating a customer record in the CRM.

Why do we need it?
- The business needs a central record for clients and account relationships.

Where is it implemented?
- `Controllers/CustomersController.cs`
- `Services/CustomerService.cs`
- `Models/Customer.cs`
- `Views/Customers/*`

Which controller handles it?
- `CustomersController`

Which model/entity represents it?
- `Customer`

Which service handles business logic?
- `CustomerService`

Which database table stores it?
- `Customers`

Which view displays it?
- `Views/Customers/Create.cshtml`, `Edit.cshtml`, `Details.cshtml`, `Index.cshtml`

Which API handles it?
- `Api/CustomersController.cs`

Which validation rules apply?
- email required and valid format
- phone required and valid format
- duplicate email and phone prevented
- customer name required

Which role can access it?
- Admin, Manager, SalesExecutive with assignment access

Which audit events are generated?
- Create, Update, Deactivate, Delete

## 2. Lead Management

What is it?
- A sales lead that may eventually become a customer.

Why do we need it?
- Leads must be tracked before conversion.

Where is it implemented?
- `Controllers/LeadsController.cs`
- `Services/LeadService.cs`
- `Models/Lead.cs`

Which controller handles it?
- `LeadsController`

Which model/entity represents it?
- `Lead`

Which service handles business logic?
- `LeadService`

Which database table stores it?
- `Leads`

Which view displays it?
- `Views/Leads/*`

Which API handles it?
- `Api/LeadsController.cs`

Which validation rules apply?
- valid email and phone
- status transitions must be allowed
- conversion requires a valid opportunity flow

Which role can access it?
- Admin, Manager, SalesExecutive with assigned leads only

Which audit events are generated?
- Create, Update, Status change, Lead conversion

## 3. Opportunity Management

What is it?
- A sales deal connected to a customer or lead.

Why do we need it?
- Opportunities represent revenue potential and pipeline value.

Where is it implemented?
- `Controllers/OpportunitiesController.cs`
- `Services/OpportunityService.cs`
- `Models/Opportunity.cs`
- `Validators/OpportunityRules.cs`

Which controller handles it?
- `OpportunitiesController`

Which model/entity represents it?
- `Opportunity`

Which service handles business logic?
- `OpportunityService`

Which database table stores it?
- `Opportunities`

Which view displays it?
- `Views/Opportunities/*`

Which API handles it?
- `Api/OpportunitiesController.cs`

Which validation rules apply?
- amount greater than zero for active opportunities
- amount cannot be negative
- probability between 0 and 100
- expected close date cannot be in the past for active opportunities

Which role can access it?
- Admin, Manager, SalesExecutive with assigned opportunities

Which audit events are generated?
- Create, Update, Stage change, Delete/Deactivate

## 4. Follow-Up Management

What is it?
- A scheduled task to contact or revisit a customer or lead.

Why do we need it?
- Sales teams need reminders and accountability.

Where is it implemented?
- `Controllers/FollowUpsController.cs`
- `Services/FollowUpService.cs`
- `Models/FollowUp.cs`
- `Validators/FollowUpRules.cs`

Which controller handles it?
- `FollowUpsController`

Which model/entity represents it?
- `FollowUp`

Which service handles business logic?
- `FollowUpService`

Which database table stores it?
- `FollowUps`

Which view displays it?
- `Views/FollowUps/*`

Which API handles it?
- Not exposed as a required API in the specification, but service logic exists for internal use.

Which validation rules apply?
- planned follow-ups cannot be earlier than today
- completion/reschedule actions update audit log

Which role can access it?
- Admin, Manager, SalesExecutive with assignment access

Which audit events are generated?
- Create, Update, Reschedule, Complete, Missed, Cancelled

## 5. Activity Management

What is it?
- A record of work such as calls, meetings, email or tasks.

Why do we need it?
- Sales activity must be tracked and attributed to a user.

Where is it implemented?
- `Controllers/ActivitiesController.cs`
- `Services/ActivityService.cs`
- `Models/Activity.cs`

Which controller handles it?
- `ActivitiesController`

Which model/entity represents it?
- `Activity`

Which service handles business logic?
- `ActivityService`

Which database table stores it?
- `Activities`

Which view displays it?
- `Views/Activities/*`

Which API handles it?
- No required public API for this entity in assignment, but it is handled in MVC and service layer.

Which validation rules apply?
- required activity type, subject, date and status
- role-based access enforced

Which role can access it?
- Admin, Manager, SalesExecutive for assigned activities

Which audit events are generated?
- Create, Update, Delete, Status change

## 6. Authentication and User Login

What is it?
- Logging in, registering, and maintaining session cookies.

Why do we need it?
- Users need secure access to CRM information.

Where is it implemented?
- `Controllers/AccountController.cs`
- `Program.cs`
- `Models/ApplicationUser.cs`

Which controller handles it?
- `AccountController`

Which model/entity represents it?
- `ApplicationUser`

Which service handles business logic?
- ASP.NET Identity `SignInManager` and `UserManager`

Which database table stores it?
- Identity tables created by EF Core and ASP.NET Identity

Which view displays it?
- `Views/Account/Login.cshtml`, `Register.cshtml`

Which API handles it?
- No direct API requirement for login; standard secured cookie auth is used.

Which validation rules apply?
- password length and complexity
- lockout after repeated failed login attempts
- active-user check

Which role can access it?
- Any authenticated user; role determines permissions

Which audit events are generated?
- LoginSuccess, LoginFailed, AccountLockout, Logout

## 7. Dashboard

What is it?
- A summary screen with KPIs and charts.

Why do we need it?
- Managers and sales staff need quick pipeline visibility.

Where is it implemented?
- `Controllers/DashboardController.cs`
- `Services/DashboardService.cs`
- `ViewModels/DashboardViewModel.cs`
- `Views/Dashboard/Index.cshtml`

Which controller handles it?
- `DashboardController`

Which model/entity represents it?
- Dashboard view model, backed by domain models

Which service handles business logic?
- `DashboardService`

Which database table stores it?
- aggregates over `Customers`, `Leads`, `Opportunities`, `FollowUps`

Which view displays it?
- `Views/Dashboard/Index.cshtml`

Which API handles it?
- No direct API; UI-only projection of authorized data

Which validation rules apply?
- none directly; role-scoped data access applies

Which role can access it?
- Admin, Manager, SalesExecutive

Which audit events are generated?
- Dashboard access is usually not explicitly audited unless required by the app logic

## 8. Reports

What is it?
- Filtered and paginated CRM summaries.

Why do we need it?
- Users need management reporting and performance views.

Where is it implemented?
- `Controllers/ReportsController.cs`
- `Services/ReportService.cs`
- `ViewModels/ReportViewModels.cs`

Which controller handles it?
- `ReportsController`

Which model/entity represents it?
- uses existing entities with report-specific view models

Which service handles business logic?
- `ReportService`

Which database table stores it?
- derived from existing CRM tables

Which view displays it?
- `Views/Reports/*`

Which API handles it?
- no required public API; report screens use MVC

Which validation rules apply?
- filters and pagination parameters validated by controller logic

Which role can access it?
- Admin, Manager, SalesExecutive depending on report type

Which audit events are generated?
- report access is not usually audited unless specifically exposed

## 9. Audit Log

What is it?
- A record of important system and business actions.

Why do we need it?
- Accountability, compliance and troubleshooting.

Where is it implemented?
- `Services/AuditService.cs`
- `Models/AuditLog.cs`
- `Controllers/AuditController.cs`

Which controller handles it?
- `AuditController`

Which model/entity represents it?
- `AuditLog`

Which service handles business logic?
- `AuditService`

Which database table stores it?
- `AuditLogs`

Which view displays it?
- `Views/Audit/*`

Which API handles it?
- not required as a public API in the assignment

Which validation rules apply?
- log actions are always recorded with user, timestamp, record id and IP address

Which role can access it?
- Admin only

Which audit events are generated?
- all major service actions record to the audit log

## 10. Security and Authorization

What is it?
- Rules that control who can do what.

Why do we need it?
- Prevent unauthorized access and ensure a salesperson only sees their assigned data.

Where is it implemented?
- `Authorization/ResourceAuthorization.cs`
- `Program.cs`
- controllers with `[Authorize]`

Which controller handles it?
- multiple controllers, with central rule checks in authorization helper

Which model/entity represents it?
- identity roles and user assignment fields

Which service handles business logic?
- `ResourceAuthorization` and Identity services

Which database table stores it?
- identity tables and business entity assignment fields

Which view displays it?
- `Views/Account/AccessDenied.cshtml`

Which API handles it?
- `Api/*.cs` controllers with `[Authorize]`

Which validation rules apply?
- role and ownership checks before each action

Which role can access it?
- defined by policy and role assignment

Which audit events are generated?
- login failures, lockouts, user administration changes, security-related actions

## 11. API Layer

What is it?
- Exposed endpoints for customer, lead and opportunity data.

Why do we need it?
- The app should provide a REST API with DTOs and secure access.

Where is it implemented?
- `Api/CustomersController.cs`
- `Api/LeadsController.cs`
- `Api/OpportunitiesController.cs`
- `DTOs/*`

Which controller handles it?
- relevant API controller

Which model/entity represents it?
- domain entities converted to DTOs

Which service handles business logic?
- service layer such as `CustomerService`, `LeadService`, `OpportunityService`

Which database table stores it?
- same CRM database tables as MVC screens

Which view displays it?
- no view; JSON response only

Which API handles it?
- the API controller itself

Which validation rules apply?
- DTO annotations and custom validation logic

Which role can access it?
- `CrmUser` policy; additional ownership restrictions apply

Which audit events are generated?
- create, update, delete operations are logged via service layer

## 12. Data and Database

What is it?
- SQLite database with EF Core.

Why do we need it?
- Persistent storage for CRM business data and identity.

Where is it implemented?
- `Data/ApplicationDbContext.cs`
- `Data/DbInitializer.cs`
- `Program.cs`

Which controller handles it?
- no direct controller; managed by EF Core and services

Which model/entity represents it?
- each domain model is a database entity

Which service handles business logic?
- EF Core and service layer coordinate access

Which database table stores it?
- each entity maps to a table

Which view displays it?
- no direct display; database is backend concern

Which API handles it?
- not directly

Which validation rules apply?
- relational constraints and data annotations

Which role can access it?
- access is role-controlled before queries are made

Which audit events are generated?
- all writes are logged

## 13. Project summary

The project combines multiple CRM concerns into a clear, interview-friendly architecture:

- identity and security
- role-based access
- service layer logic
- EF Core persistence
- validation logic
- dashboard analytics
- reporting
- audit history
- protected API endpoints

This structure is intentionally simple, maintainable and easy to extend.
