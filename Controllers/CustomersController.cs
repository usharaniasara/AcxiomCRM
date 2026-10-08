using AcxiomCRM.Authorization;
using AcxiomCRM.Constants;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.Validators;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "CrmUser")]
    public class CustomersController : CrmControllerBase
    {
        private readonly ICustomerService _customers;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomersController(ICustomerService customers, UserManager<ApplicationUser> userManager)
        {
            _customers = customers;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? status, int page = 1)
        {
            var (items, total) = await _customers.GetCustomersAsync(searchTerm, status, page, 10, CurrentUserId, CurrentRole);
            return View(new PagedResult<Customer>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                SearchTerm = searchTerm,
                Status = status
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var customer = await _customers.GetByIdAsync(id);
            if (customer == null) return NotFound();
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, customer))
            {
                return Forbid();
            }

            return View(customer);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadUsers();
            var model = new CustomerFormViewModel { Status = "Active" };
            if (CurrentRole == AppRoles.SalesExecutive)
            {
                model.AssignedToUserId = CurrentUserId;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CustomerFormViewModel model)
        {
            await ValidateUnique(model);
            if (!ModelState.IsValid)
            {
                await LoadUsers();
                return View(model);
            }

            var entity = Map(model);
            if (CurrentRole == AppRoles.SalesExecutive)
            {
                entity.AssignedToUserId = CurrentUserId;
            }

            await _customers.CreateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
            TempData["Success"] = "Customer created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var customer = await _customers.GetByIdAsync(id);
            if (customer == null) return NotFound();
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, customer))
            {
                return Forbid();
            }

            await LoadUsers();
            return View(Map(customer));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CustomerFormViewModel model)
        {
            if (id != model.CustomerId) return BadRequest();
            var existing = await _customers.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, existing))
            {
                return Forbid();
            }

            await ValidateUnique(model, id);
            if (!ModelState.IsValid)
            {
                await LoadUsers();
                return View(model);
            }

            var entity = Map(model);
            if (CurrentRole == AppRoles.SalesExecutive)
            {
                entity.AssignedToUserId = existing.AssignedToUserId ?? CurrentUserId;
            }

            await _customers.UpdateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
            TempData["Success"] = "Customer updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await _customers.GetByIdAsync(id);
            if (existing == null) return NotFound();
            if (!ResourceAuthorization.CanAccessCustomer(CurrentRole, CurrentUserId, CurrentUserName, existing))
            {
                return Forbid();
            }

            var (success, message) = await _customers.DeleteOrDeactivateAsync(id, true, CurrentUserId, CurrentUserName, ClientIp);
            TempData[success ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private async Task ValidateUnique(CustomerFormViewModel model, int? excludeId = null)
        {
            if (!ValidationHelpers.IsValidEmail(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), "Enter a valid email address.");
            }

            if (!ValidationHelpers.IsValidPhone(model.Phone))
            {
                ModelState.AddModelError(nameof(model.Phone), "Enter a valid phone number.");
            }

            if (!await _customers.IsEmailUniqueAsync(model.Email, excludeId))
            {
                ModelState.AddModelError(nameof(model.Email), "A customer with this email already exists.");
            }

            if (!await _customers.IsPhoneUniqueAsync(model.Phone, excludeId))
            {
                ModelState.AddModelError(nameof(model.Phone), "A customer with this phone already exists.");
            }
        }

        private async Task LoadUsers()
        {
            var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
            ViewBag.Statuses = new SelectList(CrmLists.CustomerStatuses);
            await Task.CompletedTask;
        }

        private static Customer Map(CustomerFormViewModel model) => new()
        {
            CustomerId = model.CustomerId,
            CustomerCode = model.CustomerCode ?? "",
            CustomerName = model.CustomerName,
            Email = model.Email,
            Phone = model.Phone,
            CompanyName = model.CompanyName,
            Address = model.Address,
            City = model.City,
            State = model.State,
            Status = model.Status,
            AssignedToUserId = model.AssignedToUserId
        };

        private static CustomerFormViewModel Map(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            CustomerCode = c.CustomerCode,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            CompanyName = c.CompanyName,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Status = c.Status,
            AssignedToUserId = c.AssignedToUserId
        };
    }
}
