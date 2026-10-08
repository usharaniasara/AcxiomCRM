using AcxiomCRM.ViewModels;

namespace AcxiomCRM.Services
{
    public interface IUserAdminService
    {
        Task<(List<UserListItemViewModel> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize);
        Task<UserListItemViewModel?> GetAsync(string id);
        Task<(bool Success, string Message, string? UserId)> CreateAsync(UserFormViewModel model, string actorId, string actorName, string? ip);
        Task<(bool Success, string Message)> UpdateAsync(UserFormViewModel model, string actorId, string actorName, string? ip);
        Task<(bool Success, string Message)> SetActiveAsync(string id, bool isActive, string actorId, string actorName, string? ip);
        Task<(bool Success, string Message)> UnlockAsync(string id, string actorId, string actorName, string? ip);
        Task<(bool Success, string Message)> ResetPasswordAsync(string id, string newPassword, string actorId, string actorName, string? ip);
    }
}
