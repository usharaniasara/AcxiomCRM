using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;

        public AuditService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task LogAsync(string userId, string userName, string action, string entityName, string recordId, string? oldValue = null, string? newValue = null, string? ipAddress = null)
        {
            var log = new AuditLog
            {
                UserId = string.IsNullOrWhiteSpace(userId) ? "Anonymous" : userId,
                UserName = string.IsNullOrWhiteSpace(userName) ? "System" : userName,
                Action = action,
                EntityName = entityName,
                RecordId = recordId ?? string.Empty,
                OldValue = oldValue,
                NewValue = newValue,
                CreatedDate = DateTime.UtcNow,
                IpAddress = ipAddress ?? "127.0.0.1"
            };

            await _context.AuditLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task<(List<AuditLog> Items, int TotalCount)> GetLogsAsync(string? userId = null, string? entityName = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 15)
        {
            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(userId))
            {
                query = query.Where(a => a.UserId == userId || a.UserName == userId);
            }

            if (!string.IsNullOrWhiteSpace(entityName))
            {
                query = query.Where(a => a.EntityName == entityName);
            }

            if (!string.IsNullOrWhiteSpace(action))
            {
                query = query.Where(a => a.Action == action);
            }

            if (startDate.HasValue)
            {
                query = query.Where(a => a.CreatedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1);
                query = query.Where(a => a.CreatedDate < end);
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }
    }
}
