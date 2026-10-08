using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.ViewModels
{
    public class FollowUpFormViewModel
    {
        public int FollowUpId { get; set; }

        [Display(Name = "Customer")]
        public int? CustomerId { get; set; }

        [Display(Name = "Lead")]
        public int? LeadId { get; set; }

        [Required(ErrorMessage = "Follow-up date is required.")]
        [DataType(DataType.Date)]
        [Display(Name = "Follow-Up Date")]
        public DateTime FollowUpDate { get; set; } = DateTime.UtcNow.Date.AddDays(1);

        [Required]
        [Display(Name = "Type")]
        public string FollowUpType { get; set; } = "Call";

        [StringLength(500)]
        public string Remarks { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Planned";

        [Display(Name = "Assigned To")]
        public string? AssignedToUserId { get; set; }
    }
}
