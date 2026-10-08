using System.Security.Claims;
using AcxiomCRM.Constants;
using AcxiomCRM.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace AcxiomCRM.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public abstract class ApiControllerBase : ControllerBase
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

        protected IActionResult ApiError(int status, string message, Dictionary<string, string[]>? errors = null)
        {
            return StatusCode(status, new ApiErrorResponse { Message = message, Errors = errors });
        }
    }
}
