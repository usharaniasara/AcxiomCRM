using System.Security.Claims;
using AcxiomCRM.Constants;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Controllers
{
    public abstract class CrmControllerBase : Controller
    {
        protected string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        protected string CurrentUserName => User.Identity?.Name ?? string.Empty;
        protected string? ClientIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        protected string CurrentRole
        {
            get
            {
                if (User.IsInRole(AppRoles.Admin)) return AppRoles.Admin;
                if (User.IsInRole(AppRoles.Manager)) return AppRoles.Manager;
                if (User.IsInRole(AppRoles.SalesExecutive)) return AppRoles.SalesExecutive;
                return string.Empty;
            }
        }
    }
}
