using AcxiomCRM.Constants;
using AcxiomCRM.Models;

namespace AcxiomCRM.Authorization
{
    /// <summary>
    /// Server-side ownership checks. Menu hiding is not security — controllers and APIs call this.
    /// </summary>
    public static class ResourceAuthorization
    {
        public static bool IsAdmin(string? role) => role == AppRoles.Admin;
        public static bool IsManager(string? role) => role == AppRoles.Manager;
        public static bool IsSales(string? role) => role == AppRoles.SalesExecutive;

        public static bool CanAccessCrm(string? role) =>
            role == AppRoles.Admin || role == AppRoles.Manager || role == AppRoles.SalesExecutive;

        public static bool CanManageUsers(string? role) => role == AppRoles.Admin;
        public static bool CanViewAudit(string? role) => role == AppRoles.Admin;
        public static bool CanViewReports(string? role) =>
            role == AppRoles.Admin || role == AppRoles.Manager || role == AppRoles.SalesExecutive;

        public static bool CanAccessAssignedRecord(string? role, string currentUserId, string? assignedToUserId)
        {
            if (role == AppRoles.Admin || role == AppRoles.Manager)
            {
                return true;
            }

            return role == AppRoles.SalesExecutive && assignedToUserId == currentUserId;
        }

        public static bool CanAccessCustomer(string? role, string currentUserId, string currentUserName, Customer customer)
        {
            if (role == AppRoles.Admin || role == AppRoles.Manager)
            {
                return true;
            }

            if (role != AppRoles.SalesExecutive)
            {
                return false;
            }

            return customer.AssignedToUserId == currentUserId
                || customer.CreatedBy == currentUserName
                || customer.CreatedBy == currentUserId;
        }
    }
}
