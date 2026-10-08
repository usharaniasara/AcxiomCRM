using System.Collections.Generic;

namespace AcxiomCRM.ViewModels
{
    public class DashboardViewModel
    {
        public string Role { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;

        // KPI Cards
        public int TotalCustomers { get; set; }
        public int TotalLeads { get; set; }
        public int OpenLeads { get; set; }
        public int TotalOpportunities { get; set; }
        public int OpenOpportunities { get; set; }
        public int WonOpportunities { get; set; }
        public int LostOpportunities { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public decimal WeightedPipelineValue { get; set; }

        // Additional Stats for Admin/Manager
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int PendingFollowUpsCount { get; set; }

        // Chart Data Lists
        public List<ChartCategoryData> LeadStatusChart { get; set; } = new();
        public List<ChartCategoryData> OpportunityStageChart { get; set; } = new();
        public List<MonthlySalesData> MonthlySalesChart { get; set; } = new();
    }

    public class ChartCategoryData
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalValue { get; set; }
    }

    public class MonthlySalesData
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int WonDealsCount { get; set; }
    }
}
