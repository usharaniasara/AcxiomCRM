using AcxiomCRM.Data;
using AcxiomCRM.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;

        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role, string userName)
        {
            var vm = new DashboardViewModel
            {
                Role = role,
                UserName = userName
            };

            // Query bases
            var customersQuery = _context.Customers.AsNoTracking();
            var leadsQuery = _context.Leads.AsNoTracking();
            var opportunitiesQuery = _context.Opportunities.AsNoTracking();
            var followUpsQuery = _context.FollowUps.AsNoTracking();

            // Role-based scoping
            if (role == "SalesExecutive")
            {
                customersQuery = customersQuery.Where(c =>
                    c.AssignedToUserId == userId
                    || c.CreatedBy == userName
                    || c.Opportunities.Any(o => o.AssignedToUserId == userId));
                leadsQuery = leadsQuery.Where(l => l.AssignedToUserId == userId);
                opportunitiesQuery = opportunitiesQuery.Where(o => o.AssignedToUserId == userId);
                followUpsQuery = followUpsQuery.Where(f => f.AssignedToUserId == userId);
            }

            // Customer counts
            vm.TotalCustomers = await customersQuery.CountAsync();

            // Lead counts
            vm.TotalLeads = await leadsQuery.CountAsync();
            vm.OpenLeads = await leadsQuery.CountAsync(l => l.Status == "New" || l.Status == "Contacted" || l.Status == "Qualified");

            // Opportunity counts & KPI Pipeline Values
            vm.TotalOpportunities = await opportunitiesQuery.CountAsync();
            vm.OpenOpportunities = await opportunitiesQuery.CountAsync(o => o.Status == "Open");
            vm.WonOpportunities = await opportunitiesQuery.CountAsync(o => o.Status == "Won");
            vm.LostOpportunities = await opportunitiesQuery.CountAsync(o => o.Status == "Lost");

            var openOpps = await opportunitiesQuery.Where(o => o.Status == "Open").ToListAsync();
            vm.TotalPipelineValue = openOpps.Sum(o => o.Amount);
            vm.WeightedPipelineValue = openOpps.Sum(o => o.Amount * (o.Probability / 100m));

            // Follow-up counts
            vm.PendingFollowUpsCount = await followUpsQuery.CountAsync(f => f.Status == "Planned");

            // Admin Stats
            if (role == "Admin" || role == "Manager")
            {
                vm.TotalUsers = await _context.Users.CountAsync();
                vm.ActiveUsers = await _context.Users.CountAsync(u => u.IsActive);
            }

            // 1. Lead Status Chart Data
            var leadStatuses = new[] { "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost" };
            var leadGroups = await leadsQuery
                .GroupBy(l => l.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var status in leadStatuses)
            {
                var match = leadGroups.FirstOrDefault(g => g.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
                vm.LeadStatusChart.Add(new ChartCategoryData
                {
                    Label = status,
                    Count = match?.Count ?? 0
                });
            }

            // 2. Opportunity Stage Chart Data
            var stages = new[] { "Qualification", "Proposal", "Negotiation", "Won", "Lost" };
            var stageGroups = await opportunitiesQuery
                .GroupBy(o => o.Stage)
                .Select(g => new { Stage = g.Key, Count = g.Count(), Value = g.Sum(x => x.Amount) })
                .ToListAsync();

            foreach (var stage in stages)
            {
                var match = stageGroups.FirstOrDefault(g => g.Stage.Equals(stage, StringComparison.OrdinalIgnoreCase));
                vm.OpportunityStageChart.Add(new ChartCategoryData
                {
                    Label = stage,
                    Count = match?.Count ?? 0,
                    TotalValue = match?.Value ?? 0
                });
            }

            // 3. Monthly Sales Chart Data (Last 6 Months)
            var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1).AddMonths(-5);
            var wonOpps = await opportunitiesQuery
                .Where(o => o.Status == "Won" && o.CreatedDate >= startDate)
                .ToListAsync();

            for (int i = 0; i < 6; i++)
            {
                var monthDate = startDate.AddMonths(i);
                var monthName = monthDate.ToString("MMM yyyy", CultureInfo.InvariantCulture);

                var monthWon = wonOpps.Where(o => o.CreatedDate.Year == monthDate.Year && o.CreatedDate.Month == monthDate.Month).ToList();

                vm.MonthlySalesChart.Add(new MonthlySalesData
                {
                    MonthName = monthName,
                    Amount = monthWon.Sum(o => o.Amount),
                    WonDealsCount = monthWon.Count
                });
            }

            return vm;
        }
    }
}
