using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Tokens;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly AppDbContext _context;

    public CreateModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = new Common.Models.Token
        {
            UserName = Input.UserName,
            TokenValue = Input.TokenValue,
            Created = DateTime.UtcNow
        };

        _context.Tokens.Add(token);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Token for '{Input.UserName}' created successfully.";
        return RedirectToPage("./Index");
    }

    public class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Token Value")]
        public string TokenValue { get; set; } = string.Empty;
    }
}
