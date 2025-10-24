using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Dashboard.Models;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Users;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteModel(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [BindProperty]
    public string Id { get; set; } = string.Empty;
    
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        Id = user.Id;
        UserName = user.UserName!;
        Email = user.Email!;
        EmailConfirmed = user.EmailConfirmed;
        Roles = (await _userManager.GetRolesAsync(user)).ToList();

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(Id))
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(Id);
        if (user == null)
        {
            return NotFound();
        }

        // Prevent deleting the current user
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["ErrorMessage"] = "You cannot delete your own account.";
            return RedirectToPage("./Index");
        }

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = $"User '{user.UserName}' deleted successfully.";
            return RedirectToPage("./Index");
        }

        TempData["ErrorMessage"] = "Failed to delete user.";
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return RedirectToPage("./Index");
    }
}
