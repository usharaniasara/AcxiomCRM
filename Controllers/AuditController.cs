using AcxiomCRM.Models;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(string? userId, string? entityName, string? action, DateTime? startDate, DateTime? endDate, int page = 1)
        {
            var (items, total) = await _auditService.GetLogsAsync(userId, entityName, action, startDate, endDate, page, 15);
            ViewBag.UserId = userId;
            ViewBag.EntityName = entityName;
            ViewBag.ActionName = action;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            return View(new PagedResult<AuditLog>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = 15
            });
        }
    }
}
