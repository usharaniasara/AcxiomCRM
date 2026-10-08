using AcxiomCRM.Constants;
using AcxiomCRM.Models;
using AcxiomCRM.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AcxiomCRM.Services
{
    public class UserAdminService : IUserAdminService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;

        public UserAdminService(UserManager<ApplicationUser> userManager, IAuditService auditService)
        {
            _userManager = userManager;
            _auditService = auditService;
        }

        public async Task<(List<UserListItemViewModel> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize)
        {
            var query = _userManager.Users.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u => u.Email!.ToLower().Contains(term) || u.FullName.ToLower().Contains(term));
            }

            var total = await query.CountAsync();
            var users = await query.OrderBy(u => u.FullName).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            var items = new List<UserListItemViewModel>();
            foreach (var user in users)
            {
                items.Add(await ToListItem(user));
            }

            return (items, total);
        }

        public async Task<UserListItemViewModel?> GetAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            return user == null ? null : await ToListItem(user);
        }

        public async Task<(bool Success, string Message, string? UserId)> CreateAsync(UserFormViewModel model, string actorId, string actorName, string? ip)
        {
            if (!AppRoles.All.Contains(model.Role))
            {
                return (false, "Invalid role.", null);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                EmailConfirmed = true,
                IsActive = model.IsActive,
                CreatedDate = DateTime.UtcNow
            };

            if (string.IsNullOrWhiteSpace(model.Password))
            {
                return (false, "Password is required.", null);
            }

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)), null);
            }

            await _userManager.AddToRoleAsync(user, model.Role);
            await _auditService.LogAsync(actorId, actorName, "Create", "User", user.Id, null, $"Created user {user.Email} with role {model.Role}", ip);
            return (true, "User created.", user.Id);
        }

        public async Task<(bool Success, string Message)> UpdateAsync(UserFormViewModel model, string actorId, string actorName, string? ip)
        {
            var user = await _userManager.FindByIdAsync(model.Id ?? "");
            if (user == null)
            {
                return (false, "User not found.");
            }

            var oldRole = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "";
            var oldValue = $"Name: {user.FullName}, Email: {user.Email}, Role: {oldRole}, Active: {user.IsActive}";

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.IsActive = model.IsActive;

            var update = await _userManager.UpdateAsync(user);
            if (!update.Succeeded)
            {
                return (false, string.Join(" ", update.Errors.Select(e => e.Description)));
            }

            if (oldRole != model.Role && AppRoles.All.Contains(model.Role))
            {
                if (!string.IsNullOrEmpty(oldRole))
                {
                    await _userManager.RemoveFromRoleAsync(user, oldRole);
                }

                await _userManager.AddToRoleAsync(user, model.Role);
                await _auditService.LogAsync(actorId, actorName, "RoleChanged", "User", user.Id, oldRole, model.Role, ip);
            }

            await _auditService.LogAsync(actorId, actorName, "Update", "User", user.Id, oldValue, $"Name: {user.FullName}, Email: {user.Email}, Role: {model.Role}, Active: {user.IsActive}", ip);
            return (true, "User updated.");
        }

        public async Task<(bool Success, string Message)> SetActiveAsync(string id, bool isActive, string actorId, string actorName, string? ip)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (false, "User not found.");
            }

            user.IsActive = isActive;
            await _userManager.UpdateAsync(user);
            await _auditService.LogAsync(actorId, actorName, isActive ? "Activate" : "Deactivate", "User", id, (!isActive).ToString(), isActive.ToString(), ip);
            return (true, isActive ? "User activated." : "User deactivated.");
        }

        public async Task<(bool Success, string Message)> UnlockAsync(string id, string actorId, string actorName, string? ip)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (false, "User not found.");
            }

            await _userManager.SetLockoutEndDateAsync(user, null);
            await _userManager.ResetAccessFailedCountAsync(user);
            await _auditService.LogAsync(actorId, actorName, "AccountUnlock", "User", id, "Locked", "Unlocked", ip);
            return (true, "Account unlocked.");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(string id, string newPassword, string actorId, string actorName, string? ip)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return (false, "User not found.");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return (false, string.Join(" ", result.Errors.Select(e => e.Description)));
            }

            await _auditService.LogAsync(actorId, actorName, "PasswordReset", "User", id, null, "Administrator reset password", ip);
            return (true, "Password reset.");
        }

        private async Task<UserListItemViewModel> ToListItem(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            return new UserListItemViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "",
                IsActive = user.IsActive,
                IsLockedOut = user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow,
                AccessFailedCount = user.AccessFailedCount,
                LockoutEnd = user.LockoutEnd,
                CreatedDate = user.CreatedDate
            };
        }
    }
}
