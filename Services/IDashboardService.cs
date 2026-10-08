using AcxiomCRM.ViewModels;
using System.Threading.Tasks;

namespace AcxiomCRM.Services
{
    public interface IDashboardService
    {
        Task<DashboardViewModel> GetDashboardDataAsync(string userId, string role, string userName);
    }
}
