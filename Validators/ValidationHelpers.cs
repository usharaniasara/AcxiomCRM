using System.Text.RegularExpressions;

namespace AcxiomCRM.Validators
{
    public static class ValidationHelpers
    {
        // Simple international-friendly phone: digits, spaces, +, -, parentheses, 8–20 chars
        private static readonly Regex PhoneRegex = new(@"^\+?[0-9\s\-()]{8,20}$", RegexOptions.Compiled);
        private static readonly Regex EmailRegex = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static bool IsValidEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email) && EmailRegex.IsMatch(email.Trim());

        public static bool IsValidPhone(string? phone) =>
            !string.IsNullOrWhiteSpace(phone) && PhoneRegex.IsMatch(phone.Trim());

        public static string RequiredMessage(string fieldName) => $"{fieldName} is required.";
    }
}
