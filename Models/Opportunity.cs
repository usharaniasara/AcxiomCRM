using System;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Opportunity
    {
        [Key]
        public int OpportunityId { get; set; }

        [Required]
        [StringLength(150)]
        public string OpportunityName { get; set; } = string.Empty;

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public virtual Lead? Lead { get; set; }

        [Range(0.01, 1000000000)]
        public decimal Amount { get; set; }

        [Required]
        [StringLength(50)]
        public string Stage { get; set; } = "Qualification"; // Qualification, Proposal, Negotiation, Won, Lost

        [Range(0, 100)]
        public int Probability { get; set; } = 20;

        public DateTime ExpectedCloseDate { get; set; } = DateTime.UtcNow.AddDays(30);

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Open"; // Open, Won, Lost

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public string? AssignedToUserId { get; set; }
        public virtual ApplicationUser? AssignedTo { get; set; }

        public string? Notes { get; set; }
    }
}
