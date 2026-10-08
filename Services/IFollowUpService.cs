using AcxiomCRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface IFollowUpService
    {
        Task<(List<FollowUp> Items, int TotalCount)> GetFollowUpsAsync(string? status = null, string? type = null, string? assignedToUserId = null, string? relatedSearch = null, bool upcomingOnly = false, bool overdueOnly = false, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null);
        Task<FollowUp?> GetByIdAsync(int id);
        Task<FollowUp> CreateAsync(FollowUp followUp, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<FollowUp> UpdateAsync(FollowUp followUp, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> UpdateStatusAsync(int followUpId, string newStatus, string? remarks, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> RescheduleAsync(int followUpId, DateTime newDate, string? remarks, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> DeleteAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null);
    }
}
