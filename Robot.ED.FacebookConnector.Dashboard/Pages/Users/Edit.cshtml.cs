using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Dashboard.Models;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Users;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public EditModel(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<string> AvailableRoles { get; set; } = new();

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

        var userRoles = await _userManager.GetRolesAsync(user);
        AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

        Input = new InputModel
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            EmailConfirmed = user.EmailConfirmed,
            SelectedRoles = userRoles.ToList()
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AvailableRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await _userManager.FindByIdAsync(Input.Id);
        if (user == null)
        {
            return NotFound();
        }

        user.Email = Input.Email;
        user.EmailConfirmed = Input.EmailConfirmed;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        // Update password if provided
        if (!string.IsNullOrEmpty(Input.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var passwordResult = await _userManager.ResetPasswordAsync(user, token, Input.NewPassword);
            if (!passwordResult.Succeeded)
            {
                foreach (var error in passwordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return Page();
            }
        }

        // Update roles
        var currentRoles = await _userManager.GetRolesAsync(user);
        var rolesToAdd = Input.SelectedRoles?.Except(currentRoles).ToList() ?? new List<string>();
        var rolesToRemove = currentRoles.Except(Input.SelectedRoles ?? new List<string>()).ToList();

        if (rolesToRemove.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
        }

        if (rolesToAdd.Any())
        {
            await _userManager.AddToRolesAsync(user, rolesToAdd);
        }

        TempData["SuccessMessage"] = $"User '{Input.UserName}' updated successfully.";
        return RedirectToPage("./Index");
    }

    public class InputModel
    {
        public string Id { get; set; } = string.Empty;

        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("NewPassword", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Email Confirmed")]
        public bool EmailConfirmed { get; set; }

        public List<string>? SelectedRoles { get; set; }
    }
}
