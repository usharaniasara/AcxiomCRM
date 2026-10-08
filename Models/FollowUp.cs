using System;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class FollowUp
    {
        [Key]
        public int FollowUpId { get; set; }

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public virtual Lead? Lead { get; set; }

        [Required]
        public DateTime FollowUpDate { get; set; }

        [Required]
        [StringLength(50)]
        public string FollowUpType { get; set; } = "Call"; // Call, Meeting, Email, Demo

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Planned"; // Planned, Completed, Missed, Cancelled

        public string? AssignedToUserId { get; set; }
        public virtual ApplicationUser? AssignedTo { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
