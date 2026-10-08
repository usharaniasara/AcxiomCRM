using AcxiomCRM.Models;

namespace AcxiomCRM.Services
{
    public interface IAuditService
    {
        Task LogAsync(string userId, string userName, string action, string entityName, string recordId, string? oldValue = null, string? newValue = null, string? ipAddress = null);
        Task<(List<AuditLog> Items, int TotalCount)> GetLogsAsync(string? userId = null, string? entityName = null, string? action = null, DateTime? startDate = null, DateTime? endDate = null, int page = 1, int pageSize = 15);
    }
}
