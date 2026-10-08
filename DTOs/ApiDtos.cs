using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.DTOs
{
    public class ApiErrorResponse
    {
        public string Message { get; set; } = "Request failed.";
        public Dictionary<string, string[]>? Errors { get; set; }
    }

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
    }

    public class CustomerWriteDto
    {
        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\+?[0-9\s\-()]{8,20}$", ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(50)]
        public string City { get; set; } = string.Empty;

        [StringLength(50)]
        public string State { get; set; } = string.Empty;

        public string Status { get; set; } = "Active";
        public string? AssignedToUserId { get; set; }
    }

    public class LeadDto
    {
        public int LeadId { get; set; }
        public string LeadCode { get; set; } = string.Empty;
        public string LeadName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal ExpectedValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? AssignedToUserId { get; set; }
        public string? AssignedToName { get; set; }
    }

    public class LeadWriteDto
    {
        [Required(ErrorMessage = "Lead Name is required.")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\+?[0-9\s\-()]{8,20}$", ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;
        public string Source { get; set; } = "Website";
        public string Status { get; set; } = "New";
        public decimal ExpectedValue { get; set; }
        public string? AssignedToUserId { get; set; }
    }

    public class OpportunityDto
    {
        public int OpportunityId { get; set; }
        public string OpportunityName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public int? LeadId { get; set; }
        public decimal Amount { get; set; }
        public string Stage { get; set; } = string.Empty;
        public int Probability { get; set; }
        public DateTime ExpectedCloseDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AssignedToUserId { get; set; }
        public decimal WeightedValue { get; set; }
    }

    public class OpportunityWriteDto
    {
        [Required(ErrorMessage = "Opportunity Name is required.")]
        public string OpportunityName { get; set; } = string.Empty;
        public int? CustomerId { get; set; }
        public int? LeadId { get; set; }

        [Range(0.01, 1000000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        public string Stage { get; set; } = "Qualification";

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; } = 20;

        public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);
        public string Status { get; set; } = "Open";
        public string? AssignedToUserId { get; set; }
        public string? Notes { get; set; }
    }
}
