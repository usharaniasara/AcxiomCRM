using AcxiomCRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface ILeadService
    {
        Task<(List<Lead> Items, int TotalCount)> GetLeadsAsync(string? searchTerm = null, string? status = null, string? assignedToUserId = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null);
        Task<Lead?> GetByIdAsync(int id);
        Task<Lead> CreateAsync(Lead lead, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<Lead> UpdateAsync(Lead lead, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> UpdateStatusAsync(int leadId, string newStatus, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message, int? CustomerId, int? OpportunityId)> ConvertLeadAsync(int leadId, string opportunityName, decimal opportunityAmount, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null);
    }
}
