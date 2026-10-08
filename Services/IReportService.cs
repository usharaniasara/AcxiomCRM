using AcxiomCRM.Models;
using AcxiomCRM.ViewModels;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface IReportService
    {
        Task<ReportResultViewModel<Customer>> GetCustomerReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<Lead>> GetLeadReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<FollowUp>> GetFollowUpReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<Opportunity>> GetOpportunityReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<Opportunity>> GetPipelineReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<LeadConversionReportItem>> GetConversionReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<UserActivityReportItem>> GetUserActivityReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
        Task<ReportResultViewModel<AuditLog>> GetAuditReportAsync(ReportFilterViewModel filter, string? currentUserId, string? currentRole);
    }
}
