using AcxiomCRM.Models;
using AcxiomCRM.Validators;

namespace AcxiomCRM.DTOs
{
    public static class DtoMapper
    {
        public static CustomerDto ToDto(Customer c) => new()
        {
            CustomerId = c.CustomerId,
            CustomerCode = c.CustomerCode,
            CustomerName = c.CustomerName,
            Email = c.Email,
            Phone = c.Phone,
            CompanyName = c.CompanyName,
            Address = c.Address,
            City = c.City,
            State = c.State,
            Status = c.Status,
            CreatedDate = c.CreatedDate,
            CreatedBy = c.CreatedBy,
            AssignedToUserId = c.AssignedToUserId
        };

        public static LeadDto ToDto(Lead l) => new()
        {
            LeadId = l.LeadId,
            LeadCode = l.LeadCode,
            LeadName = l.LeadName,
            Email = l.Email,
            Phone = l.Phone,
            CompanyName = l.CompanyName,
            Source = l.Source,
            Status = l.Status,
            ExpectedValue = l.ExpectedValue,
            CreatedDate = l.CreatedDate,
            AssignedToUserId = l.AssignedToUserId,
            AssignedToName = l.AssignedTo?.FullName
        };

        public static OpportunityDto ToDto(Opportunity o) => new()
        {
            OpportunityId = o.OpportunityId,
            OpportunityName = o.OpportunityName,
            CustomerId = o.CustomerId,
            CustomerName = o.Customer?.CustomerName,
            LeadId = o.LeadId,
            Amount = o.Amount,
            Stage = o.Stage,
            Probability = o.Probability,
            ExpectedCloseDate = o.ExpectedCloseDate,
            Status = o.Status,
            AssignedToUserId = o.AssignedToUserId,
            WeightedValue = OpportunityRules.WeightedPipeline(o.Amount, o.Probability)
        };
    }
}
