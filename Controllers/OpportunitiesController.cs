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
    public class OpportunitiesController : CrmControllerBase
    {
        private readonly IOpportunityService _opportunities;
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public OpportunitiesController(IOpportunityService opportunities, ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _opportunities = opportunities;
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? stage, string? status, int page = 1)
        {
            var (items, total) = await _opportunities.GetOpportunitiesAsync(searchTerm, stage, status, null, page, 10, CurrentUserId, CurrentRole);
            ViewBag.Stages = CrmLists.OpportunityStages;
            return View(new PagedResult<Opportunity>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                SearchTerm = searchTerm,
                Status = status,
                ExtraFilter = stage
            });
        }

        public async Task<IActionResult> Pipeline()
        {
            var (items, _) = await _opportunities.GetOpportunitiesAsync(null, null, "Open", null, 1, 100, CurrentUserId, CurrentRole);
            ViewBag.Weighted = items.Sum(o => _opportunities.CalculateWeightedValue(o.Amount, o.Probability));
            return View(items);
        }

        public async Task<IActionResult> Details(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.OpportunityId == -1) return Forbid();
            ViewBag.Weighted = _opportunities.CalculateWeightedValue(item.Amount, item.Probability);
            return View(item);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLookups();
            var model = new OpportunityFormViewModel();
            if (CurrentRole == AppRoles.SalesExecutive) model.AssignedToUserId = CurrentUserId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OpportunityFormViewModel model)
        {
            ApplyBusinessValidation(model);
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            try
            {
                var entity = Map(model);
                if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = CurrentUserId;
                await _opportunities.CreateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
                TempData["Success"] = "Opportunity created.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadLookups();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await Authorize(id);
            if (item == null) return NotFound();
            if (item.OpportunityId == -1) return Forbid();
            await LoadLookups();
            return View(Map(item));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, OpportunityFormViewModel model)
        {
            if (id != model.OpportunityId) return BadRequest();
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.OpportunityId == -1) return Forbid();
            ApplyBusinessValidation(model);
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            try
            {
                var entity = Map(model);
                if (CurrentRole == AppRoles.SalesExecutive) entity.AssignedToUserId = existing.AssignedToUserId;
                await _opportunities.UpdateAsync(entity, CurrentUserId, CurrentUserName, ClientIp);
                TempData["Success"] = "Opportunity updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadLookups();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStage(int id, string newStage)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.OpportunityId == -1) return Forbid();
            var (ok, message) = await _opportunities.UpdateStageAsync(id, newStage, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await Authorize(id);
            if (existing == null) return NotFound();
            if (existing.OpportunityId == -1) return Forbid();
            var (ok, message) = await _opportunities.DeleteOrDeactivateAsync(id, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private void ApplyBusinessValidation(OpportunityFormViewModel model)
        {
            foreach (var error in OpportunityRules.Validate(model.Amount, model.Probability, model.ExpectedCloseDate, model.Status))
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        private async Task<Opportunity?> Authorize(int id)
        {
            var item = await _opportunities.GetByIdAsync(id);
            if (item == null) return null;
            if (!ResourceAuthorization.CanAccessAssignedRecord(CurrentRole, CurrentUserId, item.AssignedToUserId))
            {
                return new Opportunity { OpportunityId = -1 };
            }

            return item;
        }

        private async Task LoadLookups()
        {
            ViewBag.Customers = new SelectList(await _db.Customers.AsNoTracking().Where(c => c.Status == "Active").OrderBy(c => c.CustomerName).ToListAsync(), "CustomerId", "CustomerName");
            ViewBag.Leads = new SelectList(await _db.Leads.AsNoTracking().OrderBy(l => l.LeadName).ToListAsync(), "LeadId", "LeadName");
            ViewBag.Users = new SelectList(_userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList(), "Id", "FullName");
            ViewBag.Stages = new SelectList(CrmLists.OpportunityStages);
            ViewBag.Statuses = new SelectList(CrmLists.OpportunityStatuses);
        }

        private static Opportunity Map(OpportunityFormViewModel m) => new()
        {
            OpportunityId = m.OpportunityId,
            OpportunityName = m.OpportunityName,
            CustomerId = m.CustomerId,
            LeadId = m.LeadId,
            Amount = m.Amount,
            Stage = m.Stage,
            Probability = m.Probability,
            ExpectedCloseDate = m.ExpectedCloseDate,
            Status = m.Status,
            AssignedToUserId = m.AssignedToUserId,
            Notes = m.Notes
        };

        private static OpportunityFormViewModel Map(Opportunity o) => new()
        {
            OpportunityId = o.OpportunityId,
            OpportunityName = o.OpportunityName,
            CustomerId = o.CustomerId,
            LeadId = o.LeadId,
            Amount = o.Amount,
            Stage = o.Stage,
            Probability = o.Probability,
            ExpectedCloseDate = o.ExpectedCloseDate,
            Status = o.Status,
            AssignedToUserId = o.AssignedToUserId,
            Notes = o.Notes
        };
    }
}
