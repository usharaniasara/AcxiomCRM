using AcxiomCRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface IOpportunityService
    {
        Task<(List<Opportunity> Items, int TotalCount)> GetOpportunitiesAsync(string? searchTerm = null, string? stage = null, string? status = null, string? assignedToUserId = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null);
        Task<Opportunity?> GetByIdAsync(int id);
        Task<Opportunity> CreateAsync(Opportunity opportunity, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<Opportunity> UpdateAsync(Opportunity opportunity, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> UpdateStageAsync(int opportunityId, string newStage, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null);
        
        // Helper calculation method explicitly defined for interview explanation & clarity
        decimal CalculateWeightedValue(decimal amount, int probability);
    }
}
