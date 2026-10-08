using AcxiomCRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface IActivityService
    {
        Task<(List<Activity> Items, int TotalCount)> GetActivitiesAsync(string? type = null, string? status = null, string? assignedToUserId = null, string? searchTerm = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null);
        Task<Activity?> GetByIdAsync(int id);
        Task<Activity> CreateAsync(Activity activity, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<Activity> UpdateAsync(Activity activity, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> UpdateStatusAsync(int activityId, string newStatus, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> DeleteAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null);
    }
}
