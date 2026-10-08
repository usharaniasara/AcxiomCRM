using System;
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public string UserId { get; set; } = string.Empty;

        [StringLength(100)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, Deactivate, LoginSuccess, LoginFailed, AccountLockout, LeadConverted, RoleChanged, PasswordReset

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        [StringLength(50)]
        public string RecordId { get; set; } = string.Empty;

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [StringLength(50)]
        public string IpAddress { get; set; } = string.Empty;
    }
}
