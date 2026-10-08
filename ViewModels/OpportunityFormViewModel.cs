using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class OpportunityFormViewModel
    {
        public int OpportunityId { get; set; }

        [Required(ErrorMessage = "Opportunity Name is required.")]
        [StringLength(150)]
        [Display(Name = "Opportunity Name")]
        public string OpportunityName { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(0.01, 1000000000, ErrorMessage = "Opportunity Amount must be greater than 0.")]
        public decimal Amount { get; set; }

        [Required]
        public string Stage { get; set; } = "Qualification";

        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100.")]
        public int Probability { get; set; } = 20;

        [Required(ErrorMessage = "Expected Close Date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Expected Close Date")]
        public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

        [Required]
        public string Status { get; set; } = "Open";

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
