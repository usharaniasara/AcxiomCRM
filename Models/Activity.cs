using System;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        [StringLength(50)]
        public string ActivityType { get; set; } = "Call"; // Call, Meeting, Email, Task

        [Required]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow;

        public int? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        public int? LeadId { get; set; }
        public virtual Lead? Lead { get; set; }

        public string? AssignedToUserId { get; set; }
        public virtual ApplicationUser? AssignedTo { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, In Progress, Completed, Cancelled

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    }
}
