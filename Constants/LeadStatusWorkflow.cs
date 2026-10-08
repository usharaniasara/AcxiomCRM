namespace AcxiomCRM.Constants
{
    /// <summary>
    /// Allowed lead status changes. Conversion to Converted is done only by ConvertLeadAsync.
    /// </summary>
    public static class LeadStatusWorkflow
    {
        private static readonly Dictionary<string, string[]> Allowed = new(StringComparer.OrdinalIgnoreCase)
        {
            ["New"] = ["Contacted", "Unqualified", "Lost"],
            ["Contacted"] = ["Qualified", "Unqualified", "Lost", "New"],
            ["Qualified"] = ["Contacted", "Unqualified", "Lost"],
            ["Unqualified"] = ["Contacted", "Lost"],
            ["Lost"] = ["New", "Contacted"],
            ["Converted"] = []
        };

        public static bool CanTransition(string currentStatus, string newStatus)
        {
            if (string.Equals(currentStatus, newStatus, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!Allowed.TryGetValue(currentStatus, out var next))
            {
                return false;
            }

            return next.Any(s => s.Equals(newStatus, StringComparison.OrdinalIgnoreCase));
        }

        public static IReadOnlyList<string> NextStatuses(string currentStatus)
        {
            if (!Allowed.TryGetValue(currentStatus, out var next))
            {
                return Array.Empty<string>();
            }

            return next;
        }
    }
}
