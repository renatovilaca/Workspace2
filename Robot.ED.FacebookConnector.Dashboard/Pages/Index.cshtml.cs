using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Dashboard.Services;
using Robot.ED.FacebookConnector.Dashboard.ViewModels;

namespace Robot.ED.FacebookConnector.Dashboard.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IDashboardService _dashboardService;

    public IndexModel(ILogger<IndexModel> logger, IDashboardService dashboardService)
    {
        _logger = logger;
        _dashboardService = dashboardService;
    }

    public DashboardViewModel ViewModel { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            ViewModel = await _dashboardService.GetDashboardDataAsync();
            
            // Log user action
            if (User.Identity?.IsAuthenticated == true)
            {
                await _dashboardService.LogUserActionAsync(
                    User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "",
                    User.Identity.Name ?? "",
                    "ViewDashboard");
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard");
            return RedirectToPage("/Error");
        }
    }
}
