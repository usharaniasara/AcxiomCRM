using AcxiomCRM.Authorization;
using AcxiomCRM.Constants;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "CrmUser")]
    public class ActivitiesController : CrmControllerBase
    {
        private readonly IActivityService _activities;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public ActivitiesController(IActivityService activities, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _activities = activities;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? type, string? status, string? assignedToUserId, string? searchTerm, int page = 1)
        {
            var (items, total) = await _activities.GetActivitiesAsync(type, status, assignedToUserId, searchTerm, page, 10, CurrentUserId, CurrentRole);
            await LoadLookups();
            return View(new PagedResult<Activity>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                SearchTerm = searchTerm,
                Status = status,
                ExtraFilter = type,
                AssignedToUserId = assignedToUserId
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.ActivityId == -1) return Forbid();
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLookups();
            var model = new ActivityFormViewModel();
            if (CurrentRole == AppRoles.SalesExecutive) model.AssignedToUserId = CurrentUserId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ActivityFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            var entity = Map(model);
            if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = CurrentUserId;
            await _activities.CreateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
            TempData["Success"] = "Activity created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.ActivityId == -1) return Forbid();
            await LoadLookups();
            return View(Map(item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ActivityFormViewModel model)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.ActivityId == -1) return Forbid();
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            var entity = Map(model);
            if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = existing.AssignedToUserId;
            await _activities.UpdateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
            TempData["Success"] = "Activity updated.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.ActivityId == -1) return Forbid();
            var (ok, message) = await _activities.UpdateStatusAsync(id, newStatus, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.ActivityId == -1) return Forbid();
            var (ok, message) = await _activities.DeleteAsync(id, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private async Task<Activity?> Authorize(int id)
        {
            var item = await _activities.GetByIdAsync(id);
            if (item == null) return null;
            if (!ResourceAuthorization.CanAccessAssignedRecord(CurrentRole, CurrentUserId, item.AssignedToUserId))
            {
                return new Activity { ActivityId = -1 };
            }

            return item;
        }

        private async Task LoadLookups()
        {
            ViewBag.Customers = new SelectList(await _db.Customers.AsNoTracking().OrderBy(c => c.CustomerName).ToListAsync(), "CustomerId", "CustomerName");
            ViewBag.Leads = new SelectList(await _db.Leads.AsNoTracking().OrderBy(l => l.LeadName).ToListAsync(), "LeadId", "LeadName");
            ViewBag.Users = new SelectList(_userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList(), "Id", "FullName");
            ViewBag.Types = new SelectList(CrmLists.ActivityTypes);
            ViewBag.Statuses = new SelectList(CrmLists.ActivityStatuses);
        }

        private static Activity Map(ActivityFormViewModel m) => new()
        {
            ActivityId = m.ActivityId,
            ActivityType = m.ActivityType,
            Subject = m.Subject,
            Description = m.Description,
            ActivityDate = m.ActivityDate,
            CustomerId = m.CustomerId,
            LeadId = m.LeadId,
            AssignedToUserId = m.AssignedToUserId,
            Status = m.Status
        };

        private static ActivityFormViewModel Map(Activity a) => new()
        {
            ActivityId = a.ActivityId,
            ActivityType = a.ActivityType,
            Subject = a.Subject,
            Description = a.Description,
            ActivityDate = a.ActivityDate,
            CustomerId = a.CustomerId,
            LeadId = a.LeadId,
            AssignedToUserId = a.AssignedToUserId,
            Status = a.Status
        };
    }
}
