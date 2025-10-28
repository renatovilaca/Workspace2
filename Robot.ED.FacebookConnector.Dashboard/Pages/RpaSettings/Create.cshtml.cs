using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Dashboard.Data;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.RpaSettings;

[Authorize(Roles = "Admin")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
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

        var setting = new Common.Models.RpaSetting
        {
            Key = Input.Key,
            Value = Input.Value,
            CreatedAt = DateTime.UtcNow
        };

        _context.RpaSettings.Add(setting);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Setting '{Input.Key}' created successfully.";
        return RedirectToPage("./Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(255)]
        [Display(Name = "Key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; } = string.Empty;
    }
}
