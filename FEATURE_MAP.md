# Feature-to-File Map

This document maps project requirements to the implementation files.

## Customer Email Uniqueness
- Requirement: customer email must be unique
- Feature: customer uniqueness check before create/edit
- Controller: `Controllers/CustomersController.cs`, `Api/CustomersController.cs`
- Service: `Services/CustomerService.cs`
- Model: `Models/Customer.cs`
- View: `Views/Customers/Create.cshtml`, `Edit.cshtml`
- Database: unique index on `Customers.Email`
- API: `Api/CustomersController.cs`
- Validation: `ValidationHelpers.IsValidEmail`, customer controller checks
- Audit: `Services/AuditService.cs`

## Customer Phone Uniqueness
- Requirement: customer phone must be unique
- Feature: customer uniqueness validation for phone numbers
- Controller: `Controllers/CustomersController.cs`, `Api/CustomersController.cs`
- Service: `Services/CustomerService.cs`
- Model: `Models/Customer.cs`
- View: `Views/Customers/Create.cshtml`, `Edit.cshtml`
- Database: unique index on `Customers.Phone`
- API: `Api/CustomersController.cs`
- Validation: `ValidationHelpers.IsValidPhone`
- Audit: service audit logging

## Lead Status Workflow
- Requirement: lead workflow must respect valid transitions and conversion
- Feature: status update and conversion logic
- Controller: `Controllers/LeadsController.cs`
- Service: `Services/LeadService.cs`
- Model: `Models/Lead.cs`
- View: `Views/Leads/Convert.cshtml`, `Views/Leads/Details.cshtml`
- Database: `Leads` table
- API: `Api/LeadsController.cs`
- Validation: controller/service checks
- Audit: `AuditLog` entry on conversion and update

## Opportunity Validation
- Requirement: amount, probability and close date validation
- Feature: business validation rules for sales deals
- Controller: `Controllers/OpportunitiesController.cs`
- Service: `Services/OpportunityService.cs`
- Model: `Models/Opportunity.cs`
- View: `Views/Opportunities/Create.cshtml`, `Edit.cshtml`
- Database: `Opportunities` table
- API: `Api/OpportunitiesController.cs`
- Validation: `Validators/OpportunityRules.cs`
- Audit: service log changes

## Follow-Up Date Rule
- Requirement: planned follow-up cannot be earlier than today
- Feature: date validation before creating or updating follow-up
- Controller: `Controllers/FollowUpsController.cs`
- Service: `Services/FollowUpService.cs`
- Model: `Models/FollowUp.cs`
- View: `Views/FollowUps/Create.cshtml`, `Edit.cshtml`
- Database: `FollowUps` table
- API: no required API
- Validation: `Validators/FollowUpRules.cs`
- Audit: `FollowUpService` logging

## Authentication and Login
- Requirement: login, logout and secure session handling
- Feature: secure login experience with Identity
- Controller: `Controllers/AccountController.cs`
- Service: `UserManager`, `SignInManager`, `IAuditService`
- Model: `Models/ApplicationUser.cs`
- View: `Views/Account/Login.cshtml`, `Register.cshtml`
- Database: Identity tables plus user data
- API: no public login API
- Validation: Identity password policy
- Audit: login success/failure, lockout, logout

## Authorization by Role
- Requirement: admin, manager and sales roles must have proper access
- Feature: role and ownership enforcement
- Controller: multiple controllers with `[Authorize]`
- Service: `Authorization/ResourceAuthorization.cs`
- Model: `Models/ApplicationUser.cs`
- View: `Views/Shared/_Layout.cshtml`
- Database: role assignments and `AssignedToUserId`
- API: `Api/*.cs` endpoints
- Validation: server-side authorization checks
- Audit: security/admin actions

## Dashboard KPI and Charts
- Requirement: dashboard cards and Chart.js charts
- Feature: role-scoped KPI data and chart data
- Controller: `Controllers/DashboardController.cs`
- Service: `Services/DashboardService.cs`
- Model: `ViewModels/DashboardViewModel.cs`
- View: `Views/Dashboard/Index.cshtml`
- Database: `Customers`, `Leads`, `Opportunities`, `FollowUps`
- API: none required
- Validation: none beyond data scoping
- Audit: no direct audit requirement for viewing dashboard

## Audit Logging
- Requirement: logs for important events
- Feature: centralized audit trail
- Controller: various controllers trigger service logging
- Service: `Services/AuditService.cs`
- Model: `Models/AuditLog.cs`
- View: `Controllers/AuditController.cs`, `Views/Audit/Index.cshtml`
- Database: `AuditLogs`
- API: no required public API
- Validation: none; audit records are system-generated
- Audit: this is the mechanism itself

## REST API
- Requirement: protected customers/leads/opportunities API endpoints
- Feature: secure JSON API with DTOs and status codes
- Controller: `Api/CustomersController.cs`, `Api/LeadsController.cs`, `Api/OpportunitiesController.cs`
- Service: `CustomerService`, `LeadService`, `OpportunityService`
- Model: `Models/*.cs`
- View: none; JSON responses
- Database: same tables as web layer
- API: same controllers
- Validation: DTO attributes + custom rules
- Audit: service-level log entries

## Reports
- Requirement: customer, lead, follow-up, opportunity, pipeline, conversion, user activity, audit report
- Feature: reporting and filters
- Controller: `Controllers/ReportsController.cs`
- Service: `Services/ReportService.cs`
- Model: `ViewModels/ReportViewModels.cs`
- View: `Views/Reports/*`
- Database: existing CRM tables
- API: none required
- Validation: filter validation and pagination
- Audit: admin access can be audited in future enhancements

## User Administration
- Requirement: admin manages users, roles and security state
- Feature: user create/edit/activate/deactivate/reset password
- Controller: `Controllers/UsersController.cs`
- Service: `Services/UserAdminService.cs`
- Model: `Models/ApplicationUser.cs`
- View: `Views/Users/*`
- Database: Identity users and roles
- API: no required API
- Validation: user and password rules
- Audit: user security events logged

## Delete / Deactivate Behavior
- Requirement: destructive actions must be confirmed and not destroy business history if related records exist
- Feature: soft deactivate with audit trail
- Controller: entity controllers
- Service: `CustomerService`, `LeadService`, `OpportunityService`, `FollowUpService`, `ActivityService`
- Model: relevant CRM entity
- View: list/details actions with confirmation
- Database: same records retained where needed
- API: relevant API delete call
- Validation: access and related-record checks
- Audit: delete/deactivate audit entries
