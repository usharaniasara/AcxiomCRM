using AcxiomCRM.Data;
using AcxiomCRM.Models;
using AcxiomCRM.Validators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public class OpportunityService : IOpportunityService
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;

        public OpportunityService(ApplicationDbContext context, IAuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public decimal CalculateWeightedValue(decimal amount, int probability)
        {
            return OpportunityRules.WeightedPipeline(amount, probability);
        }

        public async Task<(List<Opportunity> Items, int TotalCount)> GetOpportunitiesAsync(string? searchTerm = null, string? stage = null, string? status = null, string? assignedToUserId = null, int page = 1, int pageSize = 10, string? currentUserId = null, string? currentRole = null)
        {
            var query = _context.Opportunities
                .AsNoTracking()
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .Include(o => o.AssignedTo)
                .AsQueryable();

            // Scope based on user role
            if (currentRole == "SalesExecutive" && !string.IsNullOrEmpty(currentUserId))
            {
                query = query.Where(o => o.AssignedToUserId == currentUserId);
            }
            else if (!string.IsNullOrEmpty(assignedToUserId))
            {
                query = query.Where(o => o.AssignedToUserId == assignedToUserId);
            }

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.Trim().ToLower();
                query = query.Where(o => o.OpportunityName.ToLower().Contains(term)
                                      || (o.Customer != null && o.Customer.CustomerName.ToLower().Contains(term))
                                      || (o.Lead != null && o.Lead.LeadName.ToLower().Contains(term)));
            }

            if (!string.IsNullOrWhiteSpace(stage))
            {
                query = query.Where(o => o.Stage == stage);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(o => o.Status == status);
            }

            int totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(o => o.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<Opportunity?> GetByIdAsync(int id)
        {
            return await _context.Opportunities
                .Include(o => o.Customer)
                .Include(o => o.Lead)
                .Include(o => o.AssignedTo)
                .FirstOrDefaultAsync(o => o.OpportunityId == id);
        }

        public async Task<Opportunity> CreateAsync(Opportunity opportunity, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            // Business Validation
            ValidateOpportunityBusinessRules(opportunity);

            opportunity.CreatedDate = DateTime.UtcNow;
            
            // Sync Status based on Stage
            if (opportunity.Stage == "Won") opportunity.Status = "Won";
            else if (opportunity.Stage == "Lost") opportunity.Status = "Lost";
            else opportunity.Status = "Open";

            await _context.Opportunities.AddAsync(opportunity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Create",
                "Opportunity",
                opportunity.OpportunityId.ToString(),
                null,
                $"Created Opportunity '{opportunity.OpportunityName}' with Amount: ${opportunity.Amount}, Stage: {opportunity.Stage}",
                ipAddress
            );

            return opportunity;
        }

        public async Task<Opportunity> UpdateAsync(Opportunity opportunity, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.Opportunities.FindAsync(opportunity.OpportunityId);
            if (existing == null) throw new KeyNotFoundException($"Opportunity #{opportunity.OpportunityId} not found.");

            ValidateOpportunityBusinessRules(opportunity);

            string oldValue = $"Name: {existing.OpportunityName}, Stage: {existing.Stage}, Amount: ${existing.Amount}, Probability: {existing.Probability}%";

            existing.OpportunityName = opportunity.OpportunityName;
            existing.CustomerId = opportunity.CustomerId;
            existing.LeadId = opportunity.LeadId;
            existing.Amount = opportunity.Amount;
            existing.Stage = opportunity.Stage;
            existing.Probability = opportunity.Probability;
            existing.ExpectedCloseDate = opportunity.ExpectedCloseDate;
            existing.AssignedToUserId = opportunity.AssignedToUserId;
            existing.Notes = opportunity.Notes;

            if (existing.Stage == "Won") existing.Status = "Won";
            else if (existing.Stage == "Lost") existing.Status = "Lost";
            else existing.Status = "Open";

            string newValue = $"Name: {existing.OpportunityName}, Stage: {existing.Stage}, Amount: ${existing.Amount}, Probability: {existing.Probability}%";

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Update",
                "Opportunity",
                opportunity.OpportunityId.ToString(),
                oldValue,
                newValue,
                ipAddress
            );

            return existing;
        }

        public async Task<(bool Success, string Message)> UpdateStageAsync(int opportunityId, string newStage, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var existing = await _context.Opportunities.FindAsync(opportunityId);
            if (existing == null) return (false, "Opportunity not found.");

            string oldStage = existing.Stage;
            existing.Stage = newStage;

            if (newStage == "Won")
            {
                existing.Probability = 100;
                existing.Status = "Won";
            }
            else if (newStage == "Lost")
            {
                existing.Probability = 0;
                existing.Status = "Lost";
            }
            else
            {
                existing.Status = "Open";
            }

            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "StageChange",
                "Opportunity",
                opportunityId.ToString(),
                $"Stage: {oldStage}",
                $"Stage: {newStage} (Status: {existing.Status})",
                ipAddress
            );

            return (true, $"Opportunity stage updated to {newStage}.");
        }

        public async Task<(bool Success, string Message)> DeleteOrDeactivateAsync(int id, string currentUserId, string currentUserName, string? ipAddress = null)
        {
            var opportunity = await _context.Opportunities.FindAsync(id);
            if (opportunity == null) return (false, "Opportunity not found.");

            if (opportunity.Status == "Won")
            {
                // Cannot delete a won deal without soft marking lost or archiving
                opportunity.Status = "Lost";
                opportunity.Stage = "Lost";
                await _context.SaveChangesAsync();

                await _auditService.LogAsync(
                    currentUserId,
                    currentUserName,
                    "Deactivate",
                    "Opportunity",
                    id.ToString(),
                    "Status: Won",
                    "Status: Lost (Closed out)",
                    ipAddress
                );

                return (true, "Opportunity stage set to Lost.");
            }

            _context.Opportunities.Remove(opportunity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync(
                currentUserId,
                currentUserName,
                "Delete",
                "Opportunity",
                id.ToString(),
                $"Deleted Opportunity: {opportunity.OpportunityName}",
                null,
                ipAddress
            );

            return (true, "Opportunity deleted successfully.");
        }

        private void ValidateOpportunityBusinessRules(Opportunity opportunity)
        {
            var errors = OpportunityRules.Validate(
                opportunity.Amount,
                opportunity.Probability,
                opportunity.ExpectedCloseDate,
                opportunity.Status);

            if (errors.Count > 0)
            {
                throw new ArgumentException(string.Join(" ", errors));
            }
        }
    }
}
