using AcxiomCRM.Authorization;
using AcxiomCRM.Constants;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.Validators;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "CrmUser")]
    public class FollowUpsController : CrmControllerBase
    {
        private readonly IFollowUpService _followUps;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public FollowUpsController(IFollowUpService followUps, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _followUps = followUps;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? status, string? relatedSearch, string? assignedToUserId, bool upcomingOnly = false, bool overdueOnly = false, int page = 1)
        {
            var (items, total) = await _followUps.GetFollowUpsAsync(status, null, assignedToUserId, relatedSearch, upcomingOnly, overdueOnly, page, 10, CurrentUserId, CurrentRole);
            await LoadLookups();
            ViewBag.UpcomingOnly = upcomingOnly;
            ViewBag.OverdueOnly = overdueOnly;
            return View(new PagedResult<FollowUp>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                Status = status,
                SearchTerm = relatedSearch,
                AssignedToUserId = assignedToUserId
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.FollowUpId == -1) return Forbid();
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLookups();
            var model = new FollowUpFormViewModel();
            if (CurrentRole == AppRoles.SalesExecutive) model.AssignedToUserId = CurrentUserId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FollowUpFormViewModel model)
        {
            var dateError = FollowUpRules.ValidatePlannedDate(model.FollowUpDate, model.Status);
            if (dateError != null) ModelState.AddModelError(nameof(model.FollowUpDate), dateError);
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            try
            {
                var entity = Map(model);
                if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = CurrentUserId;
                await _followUps.CreateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
                TempData["Success"] = "Follow-up created.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(model.FollowUpDate), ex.Message);
                await LoadLookups();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.FollowUpId == -1) return Forbid();
            await LoadLookups();
            return View(Map(item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FollowUpFormViewModel model)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.FollowUpId == -1) return Forbid();
            var dateError = FollowUpRules.ValidatePlannedDate(model.FollowUpDate, model.Status);
            if (dateError != null) ModelState.AddModelError(nameof(model.FollowUpDate), dateError);
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            try
            {
                var entity = Map(model);
                if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = existing.AssignedToUserId;
                await _followUps.UpdateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
                TempData["Success"] = "Follow-up updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(nameof(model.FollowUpDate), ex.Message);
                await LoadLookups();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id) => await Status(id, "Completed");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Missed(int id) => await Status(id, "Missed");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id) => await Status(id, "Cancelled");

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reschedule(int id, DateTime newDate, string? remarks)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.FollowUpId == -1) return Forbid();
            var (ok, message) = await _followUps.RescheduleAsync(id, newDate, remarks, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.FollowUpId == -1) return Forbid();
            var (ok, message) = await _followUps.DeleteAsync(id, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> Status(int id, string status)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.FollowUpId == -1) return Forbid();
            var (ok, message) = await _followUps.UpdateStatusAsync(id, status, null, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task<FollowUp?> Authorize(int id)
        {
            var item = await _followUps.GetByIdAsync(id);
            if (item == null) return null;
            if (!ResourceAuthorization.CanAccessAssignedRecord(CurrentRole, CurrentUserId, item.AssignedToUserId))
            {
                return new FollowUp { FollowUpId = -1 };
            }

            return item;
        }

        private async Task LoadLookups()
        {
            ViewBag.Customers = new SelectList(await _db.Customers.AsNoTracking().OrderBy(c => c.CustomerName).ToListAsync(), "CustomerId", "CustomerName");
            ViewBag.Leads = new SelectList(await _db.Leads.AsNoTracking().OrderBy(l => l.LeadName).ToListAsync(), "LeadId", "LeadName");
            ViewBag.Users = new SelectList(_userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList(), "Id", "FullName");
            ViewBag.Types = new SelectList(CrmLists.FollowUpTypes);
            ViewBag.Statuses = new SelectList(CrmLists.FollowUpStatuses);
        }

        private static FollowUp Map(FollowUpFormViewModel m) => new()
        {
            FollowUpId = m.FollowUpId,
            CustomerId = m.CustomerId,
            LeadId = m.LeadId,
            FollowUpDate = m.FollowUpDate,
            FollowUpType = m.FollowUpType,
            Remarks = m.Remarks,
            Status = m.Status,
            AssignedToUserId = m.AssignedToUserId
        };

        private static FollowUpFormViewModel Map(FollowUp f) => new()
        {
            FollowUpId = f.FollowUpId,
            CustomerId = f.CustomerId,
            LeadId = f.LeadId,
            FollowUpDate = f.FollowUpDate,
            FollowUpType = f.FollowUpType,
            Remarks = f.Remarks,
            Status = f.Status,
            AssignedToUserId = f.AssignedToUserId
        };
    }
}
