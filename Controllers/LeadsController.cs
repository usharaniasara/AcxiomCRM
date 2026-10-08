using AcxiomCRM.Authorization;
using AcxiomCRM.Constants;
using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "CrmUser")]
    public class LeadsController : CrmControllerBase
    {
        private readonly ILeadService _leads;
        private readonly UserManager<ApplicationUser> _userManager;

        public LeadsController(ILeadService leads, UserManager<ApplicationUser> userManager)
        {
            _leads = leads;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? searchTerm, string? status, string? assignedToUserId, int page = 1)
        {
            var (items, total) = await _leads.GetLeadsAsync(searchTerm, status, assignedToUserId, page, 10, CurrentUserId, CurrentRole);
            await LoadLookups();
            return View(new PagedResult<Lead>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 10,
                SearchTerm = searchTerm,
                Status = status,
                AssignedToUserId = assignedToUserId
            });
        }

        public async Task<IActionResult> Details(int id)
        {
            var lead = await AuthorizeLead(id);
            if (lead == null) return NotFound();
            if (lead == ForbiddenLead) return Forbid();
            return View(lead);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await LoadLookups();
            var model = new LeadFormViewModel();
            if (CurrentRole == AppRoles.SalesExecutive) model.AssignedToUserId = CurrentUserId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LeadFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            var lead = Map(model);
            if (CurrentRole == AppRoles.SalesExecutive) lead.AssignedToUserId = CurrentUserId;
            await _leads.CreateAsync(lead, CurrentUserId, CurrentUserName, ClientIp);
            TempData["Success"] = "Lead created.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var lead = await AuthorizeLead(id);
            if (lead == null) return NotFound();
            if (lead == ForbiddenLead) return Forbid();
            await LoadLookups();
            return View(Map(lead));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LeadFormViewModel model)
        {
            if (id != model.LeadId) return BadRequest();
            var existing = await AuthorizeLead(id);
            if (existing == null) return NotFound();
            if (existing == ForbiddenLead) return Forbid();
            if (!ModelState.IsValid)
            {
                await LoadLookups();
                return View(model);
            }

            try
            {
                var lead = Map(model);
                if (CurrentRole == AppRoles.SalesExecutive) lead.AssignedToUserId = existing.AssignedToUserId;
                await _leads.UpdateAsync(lead, CurrentUserId, CurrentUserName, ClientIp);
                TempData["Success"] = "Lead updated.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(nameof(model.Status), ex.Message);
                await LoadLookups();
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string newStatus)
        {
            var existing = await AuthorizeLead(id);
            if (existing == null) return NotFound();
            if (existing == ForbiddenLead) return Forbid();
            var (ok, message) = await _leads.UpdateStatusAsync(id, newStatus, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpGet]
        public async Task<IActionResult> Convert(int id)
        {
            var lead = await AuthorizeLead(id);
            if (lead == null) return NotFound();
            if (lead == ForbiddenLead) return Forbid();
            return View(new LeadConvertViewModel
            {
                LeadId = lead.LeadId,
                LeadName = lead.LeadName,
                OpportunityName = $"{lead.LeadName} - Opportunity",
                OpportunityAmount = lead.ExpectedValue > 0 ? lead.ExpectedValue : 1000
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Convert(LeadConvertViewModel model)
        {
            var existing = await AuthorizeLead(model.LeadId);
            if (existing == null) return NotFound();
            if (existing == ForbiddenLead) return Forbid();
            if (!ModelState.IsValid) return View(model);

            var (ok, message, _, _) = await _leads.ConvertLeadAsync(model.LeadId, model.OpportunityName, model.OpportunityAmount, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return ok ? RedirectToAction(nameof(Index)) : View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var existing = await AuthorizeLead(id);
            if (existing == null) return NotFound();
            if (existing == ForbiddenLead) return Forbid();
            var (ok, message) = await _leads.DeleteOrDeactivateAsync(id, CurrentUserId, CurrentUserName, ClientIp);
            TempData[ok ? "Success" : "Error"] = message;
            return RedirectToAction(nameof(Index));
        }

        private static readonly Lead ForbiddenLead = new() { LeadId = -1 };

        private async Task<Lead?> AuthorizeLead(int id)
        {
            var lead = await _leads.GetByIdAsync(id);
            if (lead == null) return null;
            if (!ResourceAuthorization.CanAccessAssignedRecord(CurrentRole, CurrentUserId, lead.AssignedToUserId))
            {
                return ForbiddenLead;
            }

            return lead;
        }

        private async Task LoadLookups()
        {
            var users = _userManager.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToList();
            ViewBag.Users = new SelectList(users, "Id", "FullName");
            ViewBag.Statuses = new SelectList(CrmLists.LeadStatuses);
            ViewBag.Sources = new SelectList(CrmLists.LeadSources);
            await Task.CompletedTask;
        }

        private static Lead Map(LeadFormViewModel m) => new()
        {
            LeadId = m.LeadId,
            LeadCode = m.LeadCode ?? "",
            LeadName = m.LeadName,
            Email = m.Email,
            Phone = m.Phone,
            CompanyName = m.CompanyName,
            Source = m.Source,
            Status = m.Status,
            ExpectedValue = m.ExpectedValue,
            AssignedToUserId = m.AssignedToUserId
        };

        private static LeadFormViewModel Map(Lead l) => new()
        {
            LeadId = l.LeadId,
            LeadCode = l.LeadCode,
            LeadName = l.LeadName,
            Email = l.Email,
            Phone = l.Phone,
            CompanyName = l.CompanyName,
            Source = l.Source,
            Status = l.Status,
            ExpectedValue = l.ExpectedValue,
            AssignedToUserId = l.AssignedToUserId
        };
    }
}
