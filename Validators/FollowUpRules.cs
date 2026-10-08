namespace AcxiomCRM.Validators
{
    public static class FollowUpRules
    {
        public static string? ValidatePlannedDate(DateTime followUpDate, string status)
        {
            if (string.Equals(status, "Planned", StringComparison.OrdinalIgnoreCase)
                && followUpDate.Date < DateTime.UtcNow.Date)
            {
                return "Follow-up date cannot be earlier than today.";
            }

            return null;
        }
    }
}
