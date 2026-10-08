using AcxiomCRM.Constants;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class UsersController : CrmControllerBase
    {
        private readonly IUserAdminService _users;

        public UsersController(IUserAdminService users)
        {
            _users = users;
        }

        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            var (items, total) = await _users.SearchAsync(searchTerm, page, 10);
            return View(new PagedResult<UserListItemViewModel>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                SearchTerm = searchTerm
            });
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _users.GetAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = new SelectList(AppRoles.All);
            return View(new UserFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError(nameof(model.Password), "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(AppRoles.All);
                return View(model);
            }

            var (ok, message, _) = await _users.CreateAsync(model, CurrentUserId, CurrentUserName, ClientIp);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Roles = new SelectList(AppRoles.All);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _users.GetAsync(id);
            if (user == null) return NotFound();
            ViewBag.Roles = new SelectList(AppRoles.All);
            return View(new UserFormViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(AppRoles.All);
                return View(model);
            }

            var (ok, message) = await _users.UpdateAsync(model, CurrentUserId, CurrentUserName, ClientIp);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                ViewBag.Roles = new SelectList(AppRoles.All);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(string id)
        {
            var (ok, message) = await _users.SetActiveAsync(id, false, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(string id)
        {
            var (ok, message) = await _users.SetActiveAsync(id, true, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unlock(string id)
        {
            var (ok, message) = await _users.UnlockAsync(id, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> ResetPassword(string id)
        {
            var user = await _users.GetAsync(id);
            if (user == null) return NotFound();
            return View(new AdminResetPasswordViewModel { UserId = user.Id, Email = user.Email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(AdminResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var (ok, message) = await _users.ResetPasswordAsync(model.UserId, model.NewPassword, CurrentUserId, CurrentUserName, ClientIp);
            if (!ok)
            {
                ModelState.AddModelError(string.Empty, message);
                return View(model);
            }

            TempData["Success"] = message;
            return RedirectToAction(nameof(Details), new { id = model.UserId });
        }
    }
}
