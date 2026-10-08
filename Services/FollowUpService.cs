using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class FollowUpService : IFollowUpService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public FollowUpService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<(List<FollowUp> Items, int TotalCount)> GetFollowUpsAsync(string? status = null, string? type = null, string? assignedToUserId = null, string? relatedSearch = null, bool upcomingOnly = false, bool overdueOnly = false, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null)
        {
            var query = _context.FollowUps
                .AsNoTracking()
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .Include(f => f.AssignedTo)
                .AsQueryable();

            if (currentRole == "SalesExecutive" && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(f => f.AssignedToUserId == currentUserId);
            }
            else if (!string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(f => f.AssignedToUserId == assignedToUserId);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(f => f.Status == status);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(f => f.FollowUpType == type);
            }

            if (!string.IsNullOrWhiteSpace(relatedSearch))
            {
                var term = relatedSearch.Trim().ToLower();
                query = query.Where(f =>
                    (f.Customer != null && f.Customer.CustomerName.ToLower().Contains(term))
                    || (f.Lead != null && f.Lead.LeadName.ToLower().Contains(term)));
            }

            var now = DateTime.UtcNow;

            if (upcomingOnly)
            {
                query = query.Where(f => f.Status == "Planned" && f.FollowUpDate >= now);
            }
            else if (overdueOnly)
            {
                query = query.Where(f => f.Status == "Planned" && f.FollowUpDate < now);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(f => f.FollowUpDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<FollowUp?> GetByIdAsync(int id)
        {
            return await _context.FollowUps
                .Include(f => f.Customer)
                .Include(f => f.Lead)
                .Include(f => f.AssignedTo)
                .FirstOrDefaultAsync(f => f.FollowUpId == id);
        }

        public async Task<FollowUp> CreateAsync(FollowUp followUp, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            if (followUp.FollowUpDate.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Follow-up date cannot be earlier than today.");
            }

            followUp.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(followUp.Status)) followUp.Status = "Planned";

            await _context.FollowUps.AddAsync(followUp);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Create",
                "FollowUp",
                followUp.FollowUpId.ToString(),
                null,
                $"Created Follow-Up on {followUp.FollowUpDate:yyyy-MM-dd HH:mm} ({followUp.FollowUpType})",
                ipAddress
            );

            return followUp;
        }

        public async Task<FollowUp> UpdateAsync(FollowUp followUp, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.FollowUps.FindAsync(followUp.FollowUpId);
            if (existing == null) throw new KeyNotFoundException($"Follow-Up #{followUp.FollowUpId} not found.");

            if (existing.Status == "Planned" && followUp.FollowUpDate.Date < DateTime.UtcNow.Date)
            {
                throw new ArgumentException("Follow-up date cannot be earlier than today.");
            }

            string oldValue = $"Date: {existing.FollowUpDate:yyyy-MM-dd HH:mm}, Type: {existing.FollowUpType}, Status: {existing.Status}";

            existing.CustomerId = followUp.CustomerId;
            existing.LeadId = followUp.LeadId;
            existing.FollowUpDate = followUp.FollowUpDate;
            existing.FollowUpType = followUp.FollowUpType;
            existing.Remarks = followUp.Remarks;
            existing.Status = followUp.Status;
            existing.AssignedToUserId = followUp.AssignedToUserId;

            string newValue = $"Date: {existing.FollowUpDate:yyyy-MM-dd HH:mm}, Type: {existing.FollowUpType}, Status: {existing.Status}";

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Update",
                "FollowUp",
                followUp.FollowUpId.ToString(),
                oldValue,
                newValue,
                ipAddress
            );

            return existing;
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int followUpId, string newStatus, string? remarks, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var followUp = await _context.FollowUps.FindAsync(followUpId);
            if (followUp == null) return (false, "Follow-Up not found.");

            string oldStatus = followUp.Status;
            followUp.Status = newStatus;
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                followUp.Remarks = (followUp.Remarks + " | " + remarks).Trim(' ', '|');
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "FollowUpStatusChange",
                "FollowUp",
                followUpId.ToString(),
                $"Status: {oldStatus}",
                $"Status: {newStatus}. Remarks: {remarks}",
                ipAddress
            );

            return (true, $"Follow-Up marked as {newStatus}.");
        }

        public async Task<(bool Success, string Message)> RescheduleAsync(int followUpId, DateTime newDate, string? remarks, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            if (newDate.Date < DateTime.UtcNow.Date)
            {
                return (false, "Follow-up date cannot be earlier than today.");
            }

            var followUp = await _context.FollowUps.FindAsync(followUpId);
            if (followUp == null) return (false, "Follow-Up not found.");

            DateTime oldDate = followUp.FollowUpDate;
            followUp.FollowUpDate = newDate;
            followUp.Status = "Planned";
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                followUp.Remarks = $"Rescheduled: {remarks} (Prev date: {oldDate:yyyy-MM-dd HH:mm})";
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "FollowUpRescheduled",
                "FollowUp",
                followUpId.ToString(),
                $"Date: {oldDate:yyyy-MM-dd HH:mm}",
                $"Rescheduled to: {newDate:yyyy-MM-dd HH:mm}",
                ipAddress
            );

            return (true, $"Follow-Up rescheduled to {newDate:yyyy-MM-dd HH:mm}.");
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var followUp = await _context.FollowUps.FindAsync(id);
            if (followUp == null) return (false, "Follow-Up not found.");

            _context.FollowUps.Remove(followUp);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Delete",
                "FollowUp",
                id.ToString(),
                $"Deleted FollowUp ID #{id}",
                null,
                ipAddress
            );

            return (true, "Follow-Up deleted successfully.");
        }
    }
}
