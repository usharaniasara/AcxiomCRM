using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class ActivityFormViewModel
    {
        public int ActivityId { get; set; }

        [Required]
        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; } = "Call";

        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(150)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.UtcNow.Date;

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";
    }
}
