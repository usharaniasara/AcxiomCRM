using AcxiomCRM.Constants;
using AcxiomCRM.Services;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    [Authorize(Policy = "CrmUser")]
    public class ReportsController : CrmControllerBase
    {
        private readonly IReportService _reports;

        public ReportsController(IReportService reports)
        {
            _reports = reports;
        }

        public IActionResult Index()
        {
            ViewBag.IsAdmin = CurrentRole == AppRoles.Admin;
            ViewBag.IsManager = CurrentRole == AppRoles.Manager || CurrentRole == AppRoles.Admin;
            return View();
        }

        public async Task<IActionResult> Customers(ReportFilterViewModel filter)
        {
            filter.PageSize = filter.PageSize <= 0 ? 15 : filter.PageSize;
            var result = await _reports.GetCustomerReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        public async Task<IActionResult> Leads(ReportFilterViewModel filter)
        {
            var result = await _reports.GetLeadReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        public async Task<IActionResult> FollowUps(ReportFilterViewModel filter)
        {
            var result = await _reports.GetFollowUpReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        public async Task<IActionResult> Opportunities(ReportFilterViewModel filter)
        {
            var result = await _reports.GetOpportunityReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        public async Task<IActionResult> Pipeline(ReportFilterViewModel filter)
        {
            var result = await _reports.GetPipelineReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        public async Task<IActionResult> Conversion(ReportFilterViewModel filter)
        {
            var result = await _reports.GetConversionReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        [Authorize(Policy = "AdminOrManager")]
        public async Task<IActionResult> UserActivity(ReportFilterViewModel filter)
        {
            var result = await _reports.GetUserActivityReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }

        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Audit(ReportFilterViewModel filter)
        {
            var result = await _reports.GetAuditReportAsync(filter, CurrentUserId, CurrentRole);
            return View(result);
        }
    }
}
