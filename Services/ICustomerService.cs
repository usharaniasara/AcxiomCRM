using AcxiomCRM.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface ICustomerService
    {
        Task<(List<Customer> Items, int TotalCount)> GetCustomersAsync(string? searchTerm = null, string? status = null, int page = 1, int pageSize = 10, string? userId = null, string? role = null);
        Task<Customer?> GetByIdAsync(int id);
        Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
        Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null);
        Task<Customer> CreateAsync(Customer customer, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<Customer> UpdateAsync(Customer customer, string currentUserId, string currentUserName, string? ipAddress = null);
        Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, bool softDeactivate, string currentUserId, string currentUserName, string? ipAddress = null);
    }
}
