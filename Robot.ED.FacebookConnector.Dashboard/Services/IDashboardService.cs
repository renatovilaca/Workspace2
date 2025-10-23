using Robot.ED.FacebookConnector.Dashboard.ViewModels;

namespace Robot.ED.FacebookConnector.Dashboard.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync(DateTime? filterDate = null);
    Task LogUserActionAsync(string userId, string userName, string action, string? details = null);
}
