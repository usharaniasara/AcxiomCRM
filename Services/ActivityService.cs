using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public ActivityService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<(List<Activity> Items, int TotalCount)> GetActivitiesAsync(string? type = null, string? status = null, string? assignedToUserId = null, string? searchTerm = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null)
        {
            var query = _context.Activities
                .AsNoTracking()
                .Include(a => a.Customer)
                .Include(a => a.Lead)
                .Include(a => a.AssignedTo)
                .AsQueryable();

            if (currentRole == "SalesExecutive" && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(a => a.AssignedToUserId == currentUserId);
            }
            else if (!string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(a => a.AssignedToUserId == assignedToUserId);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(a => a.ActivityType == type);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(a => a.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(a => a.Subject.ToLower().Contains(term) || a.Description.ToLower().Contains(term));
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.ActivityDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Activity?> GetByIdAsync(int id)
        {
            return await _context.Activities
                .Include(a => a.Customer)
                .Include(a => a.Lead)
                .Include(a => a.AssignedTo)
                .FirstOrDefaultAsync(a => a.ActivityId == id);
        }

        public async Task<Activity> CreateAsync(Activity activity, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            activity.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(activity.Status)) activity.Status = "Pending";

            await _context.Activities.AddAsync(activity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Create",
                "Activity",
                activity.ActivityId.ToString(),
                null,
                $"Created Activity '{activity.Subject}' ({activity.ActivityType})",
                ipAddress
            );

            return activity;
        }

        public async Task<Activity> UpdateAsync(Activity activity, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.Activities.FindAsync(activity.ActivityId);
            if (existing == null) throw new KeyNotFoundException($"Activity #{activity.ActivityId} not found.");

            string oldValue = $"Subject: {existing.Subject}, Type: {existing.ActivityType}, Status: {existing.Status}";

            existing.ActivityType = activity.ActivityType;
            existing.Subject = activity.Subject;
            existing.Description = activity.Description;
            existing.ActivityDate = activity.ActivityDate;
            existing.CustomerId = activity.CustomerId;
            existing.LeadId = activity.LeadId;
            existing.AssignedToUserId = activity.AssignedToUserId;
            existing.Status = activity.Status;

            string newValue = $"Subject: {existing.Subject}, Type: {existing.ActivityType}, Status: {existing.Status}";

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Update",
                "Activity",
                activity.ActivityId.ToString(),
                oldValue,
                newValue,
                ipAddress
            );

            return existing;
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int activityId, string newStatus, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var activity = await _context.Activities.FindAsync(activityId);
            if (activity == null) return (false, "Activity not found.");

            string oldStatus = activity.Status;
            activity.Status = newStatus;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "StatusChange",
                "Activity",
                activityId.ToString(),
                $"Status: {oldStatus}",
                $"Status: {newStatus}",
                ipAddress
            );

            return (true, $"Activity status updated to {newStatus}.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null) return (false, "Activity not found.");

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Delete",
                "Activity",
                id.ToString(),
                $"Deleted Activity ID #{id}",
                null,
                ipAddress
            );

            return (true, "Activity deleted successfully.");
        }
    }
}
