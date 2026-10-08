using AcxiomCRM.Models;

namespace AcxiomCRM.Validators
{
    /// <summary>
    /// Opportunity business rules in one place so MVC, API, and tests share the same messages.
    /// </summary>
    public static class OpportunityRules
    {
        public static List<string> Validate(decimal amount, int probability, DateTime expectedCloseDate, string status)
        {
            var errors = new List<string>();

            if (amount < 0)
            {
                errors.Add("Opportunity Amount cannot be negative.");
            }

            var isActive = !string.Equals(status, "Won", StringComparison.OrdinalIgnoreCase)
                           && !string.Equals(status, "Lost", StringComparison.OrdinalIgnoreCase);

            if (isActive && amount <= 0)
            {
                errors.Add("Opportunity Amount must be greater than 0.");
            }

            if (probability < 0 || probability > 100)
            {
                errors.Add("Probability must be between 0 and 100.");
            }

            if (isActive && expectedCloseDate.Date < DateTime.UtcNow.Date)
            {
                errors.Add("Expected Close Date cannot be in the past.");
            }

            return errors;
        }

        public static decimal WeightedPipeline(decimal amount, int probability)
        {
            if (amount <= 0 || probability <= 0)
            {
                return 0;
            }

            return amount * probability / 100m;
        }
    }
}
