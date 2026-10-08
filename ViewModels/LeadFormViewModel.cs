using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class LeadFormViewModel
    {
        public int LeadId { get; set; }

        [StringLength(20)]
        [Display(Name = "Lead Code")]
        public string? LeadCode { get; set; }

        [Required(ErrorMessage = "Lead Name is required.")]
        [StringLength(100)]
        [Display(Name = "Lead Name")]
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required.")]
        [RegularExpression(@"^\+?[0-9\s\-()]{8,20}$", ErrorMessage = "Enter a valid phone number.")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(50)]
        public string Source { get; set; } = "Website";

        [Required]
        public string Status { get; set; } = "New";

        [Range(0, 1000000000, ErrorMessage = "Expected Value cannot be negative.")]
        [Display(Name = "Expected Value")]
        public decimal ExpectedValue { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }
    }

    public class LeadConvertViewModel
    {
        public int LeadId { get; set; }
        public string LeadName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Opportunity Name is required.")]
        [Display(Name = "Opportunity Name")]
        public string OpportunityName { get; set; } = string.Empty;

        [Range(0.01, 1000000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        [Display(Name = "Opportunity Amount")]
        public decimal OpportunityAmount { get; set; }
    }
}
