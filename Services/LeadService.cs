using AcxiomCRM.Constants;
using AcxiomCRM.Data;
using AcxiomCRM.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class LeadService : ILeadService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly ICustomerService _customerService;

        public LeadService(ApplicationDbContext context, IAuditService auditService, ICustomerService customerService)
        {
            _context = context;
            _auditService = auditService;
            _customerService = customerService;
        }

        public async Task<(List<Lead> Items, int TotalCount)> GetLeadsAsync(string? searchTerm = null, string? status = null, string? assignedToUserId = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null)
        {
            var query = _context.Leads.AsNoTracking().Include(l => l.AssignedTo).AsQueryable();

            // Role-based filtering
            if (currentRole == "SalesExecutive" && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(l => l.AssignedToUserId == currentUserId);
            }
            else if (currentRole == "Manager" && !string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(l => l.AssignedToUserId == assignedToUserId);
            }
            else if (!string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(l => l.AssignedToUserId == assignedToUserId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(l => l.LeadName.ToLower().Contains(term)
                                      || l.Email.ToLower().Contains(term)
                                      || l.Phone.ToLower().Contains(term)
                                      || l.CompanyName.ToLower().Contains(term)
                                      || l.LeadCode.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(l => l.Status == status);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Lead?> GetByIdAsync(int id)
        {
            return await _context.Leads
                .Include(l => l.AssignedTo)
                .Include(l => l.Opportunities)
                .Include(l => l.FollowUps)
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(l => l.LeadId == id);
        }

        public async Task<Lead> CreateAsync(Lead lead, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            if (string.IsNullOrWhiteSpace(lead.LeadCode))
            {
                int nextId = (await _context.Leads.MaxAsync(l => (int?)l.LeadId) ?? 1000) + 1;
                lead.LeadCode = $"LEAD-{nextId}";
            }

            lead.CreatedDate = DateTime.UtcNow;
            if (string.IsNullOrEmpty(lead.Status)) lead.Status = "New";

            await _context.Leads.AddAsync(lead);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Create",
                "Lead",
                lead.LeadId.ToString(),
                null,
                $"Created Lead: {lead.LeadName} ({lead.LeadCode})",
                ipAddress
            );

            return lead;
        }

        public async Task<Lead> UpdateAsync(Lead lead, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.Leads.FindAsync(lead.LeadId);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Lead with ID {lead.LeadId} not found.");
            }

            string oldValue = $"Name: {existing.LeadName}, Status: {existing.Status}, AssignedTo: {existing.AssignedToUserId}";

            if (!LeadStatusWorkflow.CanTransition(existing.Status, lead.Status)
                && !string.Equals(existing.Status, lead.Status, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException($"Cannot change lead status from '{existing.Status}' to '{lead.Status}'.");
            }

            if (lead.Status == "Converted" && existing.Status != "Converted")
            {
                throw new InvalidOperationException("To convert a lead, please use the Convert Lead workflow.");
            }

            existing.LeadName = lead.LeadName;
            existing.Email = lead.Email;
            existing.Phone = lead.Phone;
            existing.CompanyName = lead.CompanyName;
            existing.Source = lead.Source;
            existing.Status = lead.Status;
            existing.ExpectedValue = lead.ExpectedValue;
            existing.AssignedToUserId = lead.AssignedToUserId;

            string newValue = $"Name: {existing.LeadName}, Status: {existing.Status}, AssignedTo: {existing.AssignedToUserId}";

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Update",
                "Lead",
                lead.LeadId.ToString(),
                oldValue,
                newValue,
                ipAddress
            );

            return existing;
        }

        public async Task<(bool Success, string Message)> UpdateStatusAsync(int leadId, string newStatus, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return (false, "Lead not found.");

            // Status transition validation rules
            if (lead.Status == "Converted")
            {
                return (false, "Lead is already converted and cannot change status.");
            }

            if (newStatus == "Converted")
            {
                return (false, "To convert a lead, please use the 'Convert Lead' workflow.");
            }

            if (!LeadStatusWorkflow.CanTransition(lead.Status, newStatus))
            {
                return (false, $"Cannot change lead status from '{lead.Status}' to '{newStatus}'.");
            }

            string oldStatus = lead.Status;
            lead.Status = newStatus;
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "StatusChange",
                "Lead",
                leadId.ToString(),
                $"Status: {oldStatus}",
                $"Status: {newStatus}",
                ipAddress
            );

            return (true, $"Lead status updated to {newStatus}.");
        }

        public async Task<(bool Success, string Message, int? CustomerId, int? OpportunityId)> ConvertLeadAsync(int leadId, string opportunityName, decimal opportunityAmount, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var lead = await _context.Leads.FindAsync(leadId);
            if (lead == null) return (false, "Lead not found.", null, null);

            if (lead.Status == "Converted")
            {
                return (false, "This lead has already been converted.", null, null);
            }

            if (lead.Status == "Unqualified" || lead.Status == "Lost")
            {
                return (false, $"Cannot convert a lead with status '{lead.Status}'. Please qualify the lead first.", null, null);
            }

            // 1. Create or Find Customer
            var existingCustomer = await _context.Customers.FirstOrDefaultAsync(c => c.Email.ToLower() == lead.Email.ToLower() || c.Phone == lead.Phone);
            Customer customer;

            if (existingCustomer != null)
            {
                customer = existingCustomer;
            }
            else
            {
                int nextCustId = (await _context.Customers.MaxAsync(c => (int?)c.CustomerId) ?? 1000) + 1;
                customer = new Customer
                {
                    CustomerCode = $"CUST-{nextCustId}",
                    CustomerName = lead.LeadName,
                    Email = lead.Email,
                    Phone = lead.Phone,
                    CompanyName = lead.CompanyName,
                    Status = "Active",
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = currentUserName
                };

                await _context.Customers.AddAsync(customer);
                await _context.SaveChangesAsync();
            }

            // 2. Create Opportunity
            var opportunity = new Opportunity
            {
                OpportunityName = string.IsNullOrWhiteSpace(opportunityName) ? $"{lead.LeadName} - Sales Deal" : opportunityName,
                CustomerId = customer.CustomerId,
                LeadId = lead.LeadId,
                Amount = opportunityAmount > 0 ? opportunityAmount : (lead.ExpectedValue > 0 ? lead.ExpectedValue : 10000),
                Stage = "Qualification",
                Probability = 20,
                ExpectedCloseDate = DateTime.UtcNow.AddDays(30),
                Status = "Open",
                CreatedDate = DateTime.UtcNow,
                AssignedToUserId = lead.AssignedToUserId ?? currentUserId,
                Notes = $"Converted from Lead #{lead.LeadCode} ({lead.LeadName})."
            };

            await _context.Opportunities.AddAsync(opportunity);

            // 3. Mark Lead as Converted
            string oldLeadStatus = lead.Status;
            lead.Status = "Converted";

            await _context.SaveChangesAsync();

            // 4. Record Audit Log for Lead Conversion
            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "LeadConverted",
                "Lead",
                lead.LeadId.ToString(),
                $"Status: {oldLeadStatus}",
                $"Converted to Customer ID: {customer.CustomerId}, Opportunity ID: {opportunity.OpportunityId}",
                ipAddress
            );

            return (true, "Lead successfully converted to Customer and Opportunity!", customer.CustomerId, opportunity.OpportunityId);
        }

        public async Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var lead = await _context.Leads
                .Include(l => l.Opportunities)
                .Include(l => l.FollowUps)
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(l => l.LeadId == id);

            if (lead == null) return (false, "Lead not found.");

            bool hasRelations = lead.Opportunities.Any() || lead.FollowUps.Any() || lead.Activities.Any();

            if (hasRelations || lead.Status == "Converted")
            {
                lead.Status = "Lost";
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUserId,
                    currentUserName,
                    "Deactivate",
                    "Lead",
                    id.ToString(),
                    "Status Active/Qualified",
                    "Status: Lost (Soft Deactivated due to existing conversion/activity history)",
                    ipAddress
                );

                return (true, "Lead status updated to 'Lost' to preserve activity history.");
            }

            _context.Leads.Remove(lead);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Delete",
                "Lead",
                id.ToString(),
                $"Deleted Lead: {lead.LeadName}",
                null,
                ipAddress
            );

            return (true, "Lead deleted successfully.");
        }
    }
}
