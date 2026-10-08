# Change Map

This document tells you where to modify the project when you receive a new requirement.

## 1. Adding a new field

1. Model
   - `Models/Customer.cs` or the relevant entity file
2. Database
   - add migration with `dotnet ef migrations add <Name>`
3. ViewModel / DTO
   - relevant file under `ViewModels/` or `DTOs/`
4. Controller / Service
   - update create/edit handling and mapping in the relevant controller/service
5. View
   - update form and display pages in `Views/`
6. Validation
   - update `Validators/` or model `[Required]`/range rules
7. API
   - update `Api/*Controller.cs` and `DTOs/*.cs`
8. Audit
   - ensure important changes are logged via `Services/AuditService.cs`

## 2. Deleting a field

1. Remove from the model/entity
2. Remove or update EF Core migration
3. Update view models and DTO fields
4. Update controller mapping and validation
5. Update database seed data if needed
6. Update any report logic that aggregates that field
7. Check audit/logging impact

## 3. Adding a new role

1. Add role constant in `Constants/AppRoles.cs`
2. Seed role in `Data/DbInitializer.cs`
3. Update authorization policies in `Program.cs`
4. Add role checks in controllers and views
5. Update `ResourceAuthorization.cs` if needed
6. Update documentation and role-based UI logic

## 4. Changing permissions

1. Update `Authorization/ResourceAuthorization.cs`
2. Update `Program.cs` policy definitions
3. Add or remove `[Authorize(Policy = ...)]` on controllers
4. Check dashboard and report authorization
5. Update any API authorization rules

## 5. Adding a new CRM status

1. Add the status to the relevant models and constants list
2. Update `Constants/CrmLists.cs` if the app uses centralized status lists
3. Update validation and workflow checks
4. Update UI dropdowns in relevant form views
5. Update dashboard/report filters

## 6. Changing validation

1. Update business validation in `Validators/`
2. Update MVC model-level validation when needed
3. Update DTO validation attributes for API inputs
4. Update corresponding controller logic to surface friendly messages
5. Add or adjust tests in `AcxiomCRM.Tests/`

## 7. Adding a new API endpoint

1. Add controller method in the right `Api/` controller
2. Create or update DTOs in `DTOs/`
3. Add authorization check in the controller or service
4. Validate input and return correct status codes
5. Add audit logging when the endpoint creates or changes data
6. Document the endpoint in `README.md`

## 8. Adding a dashboard card

1. Update `ViewModels/DashboardViewModel.cs`
2. Update `Services/DashboardService.cs`
3. Update `Views/Dashboard/Index.cshtml`
4. Check role-specific visibility and permission rules
5. Ensure the new metric is authorized and tested

## 9. Adding a chart

1. Add chart data in `DashboardViewModel` and `DashboardService`
2. Add `canvas` in `Views/Dashboard/Index.cshtml`
3. Add Chart.js configuration in the same view section
4. Confirm the data is filtered by role and user assignment

## 10. Adding a report

1. Add report model in `ViewModels/ReportViewModels.cs`
2. Update `Services/ReportService.cs`
3. Add controller action in `Controllers/ReportsController.cs`
4. Add the Razor view in `Views/Reports/`
5. Add authorization rules and filters
6. Add an audit or access trace if required

## 11. Changing database relationships

1. Adjust EF Core relationships in `Data/ApplicationDbContext.cs`
2. Update entity models and navigation properties
3. Generate migration
4. Check cascade and delete behavior
5. Update data queries and authorization logic
6. Review audit expectations where history must be preserved

## 12. Changing delete behavior

1. Update the delete/deactivate logic in the relevant service
2. Review `DeleteOrDeactivateAsync` methods in services
3. Ensure audit entries are added before/after the change
4. Check whether the business requires soft delete instead of hard delete
5. Update the UI confirmation message in the view

## 13. Adding a new module

1. Create a new model in `Models/`
2. Add DbSet and relationships in `Data/ApplicationDbContext.cs`
3. Add the service interface and implementation in `Services/`
4. Add controller in `Controllers/` or `Api/`
5. Add view models and Razor views in `ViewModels/` and `Views/`
6. Add validation and authorization rules
7. Wire up DI in `Program.cs`
8. Add audit logging and tests
