using AcxiomCRM.Constants;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;
        private readonly IOpportunityService _opportunityService;

        public ReportService(ApplicationDbContext context, IOpportunityService opportunityService)
        {
            _context = context;
            _opportunityService = opportunityService;
        }

        public async Task<ReportResultViewModel<Customer>> GetCustomerReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = _context.Customers.AsNoTracking().Include(c => c.AssignedTo).AsQueryable();
            if (currentRole == AppRoles.SalesExecutive && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(c => c.AssignedToUserId == currentUserId || c.CreatedBy == currentUserId);
            }

            query = ApplyDates(query, filter, c => c.CreatedDate);
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(c => c.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(c => c.CustomerName.ToLower().Contains(term) || c.CompanyName.ToLower().Contains(term) || c.Email.ToLower().Contains(term));
            }

            return await PageAsync(query.OrderByDescending(c => c.CreatedDate), filter, "Customer Report");
        }

        public async Task<ReportResultViewModel<Lead>> GetLeadReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = ScopeLeads(_context.Leads.AsNoTracking().Include(l => l.AssignedTo), currentUserId, currentRole, filter.AssignedToUserId);
            query = ApplyDates(query, filter, l => l.CreatedDate);
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(l => l.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(l => l.LeadName.ToLower().Contains(term) || l.CompanyName.ToLower().Contains(term));
            }

            return await PageAsync(query.OrderByDescending(l => l.CreatedDate), filter, "Lead Report");
        }

        public async Task<ReportResultViewModel<FollowUp>> GetFollowUpReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = _context.FollowUps.AsNoTracking()
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .Include(f => f.AssignedTo)
                .AsQueryable();

            if (currentRole == AppRoles.SalesExecutive && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(f => f.AssignedToUserId == currentUserId);
            }
            else if (!string.IsNullOrWhiteSpace(filter.AssignedToUserId))
            {
                query = query.Where(f => f.AssignedToUserId == filter.AssignedToUserId);
            }

            query = ApplyDates(query, filter, f => f.FollowUpDate);
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(f => f.Status == filter.Status);
            }

            return await PageAsync(query.OrderByDescending(f => f.FollowUpDate), filter, "Follow-Up Report");
        }

        public async Task<ReportResultViewModel<Opportunity>> GetOpportunityReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = ScopeOpps(_context.Opportunities.AsNoTracking().Include(o => o.Customer).Include(o => o.AssignedTo), currentUserId, currentRole, filter.AssignedToUserId);
            query = ApplyDates(query, filter, o => o.CreatedDate);
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(o => o.Status == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.Stage))
            {
                query = query.Where(o => o.Stage == filter.Stage);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(o => o.OpportunityName.ToLower().Contains(term));
            }

            var result = await PageAsync(query.OrderByDescending(o => o.CreatedDate), filter, "Opportunity Report");
            result.SummaryTotalValue = result.Items.Sum(o => o.Amount);
            result.SummaryWeightedValue = result.Items.Sum(o => _opportunityService.CalculateWeightedValue(o.Amount, o.Probability));
            return result;
        }

        public async Task<ReportResultViewModel<Opportunity>> GetPipelineReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = ScopeOpps(_context.Opportunities.AsNoTracking().Include(o => o.Customer).Include(o => o.AssignedTo), currentUserId, currentRole, filter.AssignedToUserId)
                .Where(o => o.Status == "Open");

            if (!string.IsNullOrWhiteSpace(filter.Stage))
            {
                query = query.Where(o => o.Stage == filter.Stage);
            }

            var result = await PageAsync(query.OrderByDescending(o => o.CreatedDate), filter, "Pipeline Report");
            result.SummaryTotalValue = result.Items.Sum(o => o.Amount);
            result.SummaryWeightedValue = result.Items.Sum(o => _opportunityService.CalculateWeightedValue(o.Amount, o.Probability));
            return result;
        }

        public async Task<ReportResultViewModel<LeadConversionReportItem>> GetConversionReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            var query = ScopeLeads(_context.Leads.AsNoTracking().Include(l => l.AssignedTo), currentUserId, currentRole, filter.AssignedToUserId);
            query = ApplyDates(query, filter, l => l.CreatedDate);
            var total = await query.CountAsync();
            var items = await query.OrderByDescending(l => l.CreatedDate)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(l => new LeadConversionReportItem
                {
                    LeadId = l.LeadId,
                    LeadCode = l.LeadCode,
                    LeadName = l.LeadName,
                    Source = l.Source,
                    Status = l.Status,
                    ExpectedValue = l.ExpectedValue,
                    CreatedDate = l.CreatedDate,
                    AssignedToName = l.AssignedTo != null ? l.AssignedTo.FullName : "",
                    IsConverted = l.Status == "Converted"
                })
                .ToListAsync();

            return new ReportResultViewModel<LeadConversionReportItem>
            {
                ReportTitle = "Conversion Report",
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Filter = filter
            };
        }

        public async Task<ReportResultViewModel<UserActivityReportItem>> GetUserActivityReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            if (currentRole == AppRoles.SalesExecutive)
            {
                return new ReportResultViewModel<UserActivityReportItem>
                {
                    ReportTitle = "User Activity Report",
                    Filter = filter,
                    Page = 1,
                    PageSize = filter.PageSize
                };
            }

            var users = await _context.Users.AsNoTracking().ToListAsync();
            var items = new List<UserActivityReportItem>();
            foreach (var user in users)
            {
                items.Add(new UserActivityReportItem
                {
                    UserId = user.Id,
                    UserName = user.FullName,
                    AssignedLeadsCount = await _context.Leads.CountAsync(l => l.AssignedToUserId == user.Id),
                    AssignedOpportunitiesCount = await _context.Opportunities.CountAsync(o => o.AssignedToUserId == user.Id),
                    CompletedFollowUpsCount = await _context.FollowUps.CountAsync(f => f.AssignedToUserId == user.Id && f.Status == "Completed"),
                    CompletedActivitiesCount = await _context.Activities.CountAsync(a => a.AssignedToUserId == user.Id && a.Status == "Completed"),
                    WonDealsTotal = await _context.Opportunities.Where(o => o.AssignedToUserId == user.Id && o.Status == "Won").SumAsync(o => (decimal?)o.Amount) ?? 0
                });
            }

            return new ReportResultViewModel<UserActivityReportItem>
            {
                ReportTitle = "User Activity Report",
                Items = items.OrderByDescending(i => i.WonDealsTotal).ToList(),
                TotalCount = items.Count,
                Page = 1,
                PageSize = items.Count == 0 ? filter.PageSize : items.Count,
                Filter = filter,
                SummaryTotalValue = items.Sum(i => i.WonDealsTotal)
            };
        }

        public async Task<ReportResultViewModel<AuditLog>> GetAuditReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole)
        {
            if (currentRole != AppRoles.Admin)
            {
                return new ReportResultViewModel<AuditLog> { ReportTitle = "Audit Report", Filter = filter };
            }

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();
            query = ApplyDates(query, filter, a => a.CreatedDate);
            if (!string.IsNullOrWhiteSpace(filter.Status))
            {
                query = query.Where(a => a.Action == filter.Status);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.Trim().ToLower();
                query = query.Where(a => a.EntityName.ToLower().Contains(term) || a.Action.ToLower().Contains(term) || a.UserName.ToLower().Contains(term));
            }

            return await PageAsync(query.OrderByDescending(a => a.CreatedDate), filter, "Audit Report");
        }

        private static IQueryable<Lead> ScopeLeads(IQueryable<Lead> query, string? userId, string? role, string? assignedTo)
        {
            if (role == AppRoles.SalesExecutive && !string.IsNullOrEmpty(userId))
            {
                return query.Where(l => l.AssignedToUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                return query.Where(l => l.AssignedToUserId == assignedTo);
            }

            return query;
        }

        private static IQueryable<Opportunity> ScopeOpps(IQueryable<Opportunity> query, string? userId, string? role, string? assignedTo)
        {
            if (role == AppRoles.SalesExecutive && !string.IsNullOrEmpty(userId))
            {
                return query.Where(o => o.AssignedToUserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(assignedTo))
            {
                return query.Where(o => o.AssignedToUserId == assignedTo);
            }

            return query;
        }

        private static IQueryable<T> ApplyDates<T>(IQueryable<T> query, ReportFilterViewModel filter, System.Linq.Expressions.Expression<Func<T, DateTime>> selector)
        {
            if (filter.StartDate.HasValue)
            {
                var start = filter.StartDate.Value;
                query = query.Where(BuildDateCompare(selector, start, true));
            }

            if (filter.EndDate.HasValue)
            {
                var end = filter.EndDate.Value.Date.AddDays(1);
                query = query.Where(BuildDateCompare(selector, end, false));
            }

            return query;
        }

        private static System.Linq.Expressions.Expression<Func<T, bool>> BuildDateCompare<T>(System.Linq.Expressions.Expression<Func<T, DateTime>> selector, DateTime value, bool greaterOrEqual)
        {
            var param = selector.Parameters[0];
            var body = greaterOrEqual
                ? System.Linq.Expressions.Expression.GreaterThanOrEqual(selector.Body, System.Linq.Expressions.Expression.Constant(value))
                : System.Linq.Expressions.Expression.LessThan(selector.Body, System.Linq.Expressions.Expression.Constant(value));
            return System.Linq.Expressions.Expression.Lambda<Func<T, bool>>(body, param);
        }

        private static async Task<ReportResultViewModel<T>> PageAsync<T>(IOrderedQueryable<T> query, ReportFilterViewModel filter, string title)
        {
            var total = await query.CountAsync();
            var items = await query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize).ToListAsync();
            return new ReportResultViewModel<T>
            {
                ReportTitle = title,
                Items = items,
                TotalCount = total,
                Page = filter.Page,
                PageSize = filter.PageSize,
                Filter = filter
            };
        }
    }
}
