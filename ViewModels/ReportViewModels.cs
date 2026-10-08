using AcxiomCRM.Models;
using System;
using System.Collections.Generic;

namespace AcxiomCRM.ViewModels
{
    public class ReportFilterViewModel
    {
        public string ReportType { get; set; } = "Customer"; // Customer, Lead, FollowUp, Opportunity, Pipeline, Conversion, UserActivity, Audit
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Status { get; set; }
        public string? Stage { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? SearchTerm { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
    }

    public class ReportResultViewModel<T>
    {
        public string ReportTitle { get; set; } = string.Empty;
        public List<T> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 15;
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public ReportFilterViewModel Filter { get; set; } = new();
        
        // Summary metrics
        public decimal SummaryTotalValue { get; set; }
        public decimal SummaryWeightedValue { get; set; }
    }

    public class LeadConversionReportItem
    {
        public int LeadId { get; set; }
        public string LeadCode { get; set; } = string.Empty;
        public string LeadName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public string AssignedToName { get; set; } = string.Empty;
        public bool IsConverted { get; set; }
    }

    public class UserActivityReportItem
    {
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int AssignedLeadsCount { get; set; }
        public int AssignedOpportunitiesCount { get; set; }
        public int CompletedFollowUpsCount { get; set; }
        public int CompletedActivitiesCount { get; set; }
        public decimal WonDealsTotal { get; set; }
    }
}
