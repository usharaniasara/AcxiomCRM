using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public CustomerService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<(List<Customer> Items, int TotalCount)> GetCustomersAsync(string? searchTerm = null, string? status = null, int page = 1, int pageSize = 10, string? userId = null, string? role = null)
        {
            var query = _context.Customers.AsNoTracking().Include(c => c.AssignedTo).AsQueryable();

            if (role == "SalesExecutive" && !string.IsNullOrEmpty(userId))
            {
                query = query.Where(c =>
                    c.AssignedToUserId == userId
                    || c.CreatedBy == userId
                    || c.Opportunities.Any(o => o.AssignedToUserId == userId));
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(c => c.CustomerName.ToLower().Contains(term)
                                      || c.Email.ToLower().Contains(term)
                                      || c.Phone.ToLower().Contains(term)
                                      || c.CompanyName.ToLower().Contains(term)
                                      || c.CustomerCode.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(c => c.Status == status);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.AssignedTo)
                .Include(c => c.Opportunities)
                .Include(c => c.FollowUps)
                .Include(c => c.Activities)
                .FirstOrDefaultAsync(c => c.CustomerId == id);
        }

        public async Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(email)) return true;
            var query = _context.Customers.AsNoTracking().Where(c => c.Email.ToLower() == email.Trim().ToLower());
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CustomerId != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<bool> IsPhoneUniqueAsync(string phone, int? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true;
            var query = _context.Customers.AsNoTracking().Where(c => c.Phone == phone.Trim());
            if (excludeId.HasValue)
            {
                query = query.Where(c => c.CustomerId != excludeId.Value);
            }
            return !await query.AnyAsync();
        }

        public async Task<Customer> CreateAsync(Customer customer, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            // Auto generate CustomerCode if not provided
            if (string.IsNullOrWhiteSpace(customer.CustomerCode))
            {
                int nextId = (await _context.Customers.MaxAsync(c => (int?)c.CustomerId) ?? 1000) + 1;
                customer.CustomerCode = $"CUST-{nextId}";
            }

            customer.CreatedDate = DateTime.UtcNow;
            customer.CreatedBy = currentUserName;

            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Create",
                "Customer",
                customer.CustomerId.ToString(),
                null,
                $"Created Customer: {customer.CustomerName} ({customer.CustomerCode})",
                ipAddress
            );

            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.Customers.FindAsync(customer.CustomerId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Customer with ID {customer.CustomerId} not found.");
            }

            string oldValue = $"Name: {existing.CustomerName}, Email: {existing.Email}, Phone: {existing.Phone}, Status: {existing.Status}";

            existing.CustomerName = customer.CustomerName;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.CompanyName = customer.CompanyName;
            existing.Address = customer.Address;
            existing.City = customer.City;
            existing.State = customer.State;
            existing.Status = customer.Status;
            existing.AssignedToUserId = customer.AssignedToUserId;

            string newValue = $"Name: {existing.CustomerName}, Email: {existing.Email}, Phone: {existing.Phone}, Status: {existing.Status}";

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Update",
                "Customer",
                customer.CustomerId.ToString(),
                oldValue,
                newValue,
                ipAddress
            );

            return existing;
        }

        public async Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, bool softDeactivate, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var customer = await _context.Customers
                .Include(c => c.Opportunities)
                .Include(c => c.FollowUps)
                .Include(c => c.Activities)
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer == null)
            {
                return (false, "Customer not found.");
            }

            bool hasRelatedRecords = customer.Opportunities.Any() || customer.FollowUps.Any() || customer.Activities.Any();

            if (softDeactivate || hasRelatedRecords)
            {
                // Soft deactivate to preserve business history
                customer.Status = "Inactive";
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUserId,
                    currentUserName,
                    "Deactivate",
                    "Customer",
                    id.ToString(),
                    "Status: Active",
                    "Status: Inactive (Soft Deactivated due to business history)",
                    ipAddress
                );

                return (true, hasRelatedRecords 
                    ? "Customer has linked opportunities/follow-ups, so status was updated to Inactive to preserve audit history." 
                    : "Customer status changed to Inactive.");
            }
            else
            {
                // Hard delete if no relations exist
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUserId,
                    currentUserName,
                    "Delete",
                    "Customer",
                    id.ToString(),
                    $"Deleted Customer: {customer.CustomerName}",
                    null,
                    ipAddress
                );

                return (true, "Customer deleted successfully.");
            }
        }
    }
}
