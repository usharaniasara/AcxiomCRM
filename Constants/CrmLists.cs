namespace AcxiomCRM.Constants
{
    /// <summary>
    /// Status and lookup lists used by forms, validation, charts, and workflows.
    /// Edit these arrays to add or rename CRM statuses.
    /// </summary>
    public static class CrmLists
    {
        public static readonly string[] CustomerStatuses = { "Active", "Inactive" };

        public static readonly string[] LeadStatuses =
        {
            "New", "Contacted", "Qualified", "Unqualified", "Converted", "Lost"
        };

        public static readonly string[] LeadSources =
        {
            "Website", "Referral", "Cold Call", "Trade Show", "Organic", "Other"
        };

        public static readonly string[] OpportunityStages =
        {
            "Qualification", "Proposal", "Negotiation", "Won", "Lost"
        };

        public static readonly string[] OpportunityStatuses = { "Open", "Won", "Lost" };

        public static readonly string[] FollowUpTypes = { "Call", "Meeting", "Email", "Demo" };

        public static readonly string[] FollowUpStatuses = { "Planned", "Completed", "Missed", "Cancelled" };

        public static readonly string[] ActivityTypes = { "Call", "Meeting", "Email", "Task" };

        public static readonly string[] ActivityStatuses = { "Pending", "In Progress", "Completed", "Cancelled" };

        public static readonly string[] OpenLeadStatuses = { "New", "Contacted", "Qualified" };
        public static readonly string[] OpenOpportunityStages = { "Qualification", "Proposal", "Negotiation" };
    }
}
