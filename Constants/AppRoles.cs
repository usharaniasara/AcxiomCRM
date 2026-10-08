namespace AcxiomCRM.Constants
{
    /// <summary>
    /// Primary CRM roles. Change names here, then update seed data and [Authorize] attributes.
    /// </summary>
    public static class AppRoles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string SalesExecutive = "SalesExecutive";

        public static readonly string[] All = { Admin, Manager, SalesExecutive };
        public static readonly string[] CrmUsers = { Admin, Manager, SalesExecutive };
        public static readonly string[] AdminOrManager = { Admin, Manager };
    }
}
