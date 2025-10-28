using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Robots;

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

        var robot = new Common.Models.Robot
        {
            Name = Input.Name,
            Url = Input.Url,
            Available = Input.Available,
            Token = Input.Token,
            CreatedAt = DateTime.UtcNow
        };

        _context.Robots.Add(robot);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Robot '{Input.Name}' created successfully.";
        return RedirectToPage("./Index");
    }

    public class InputModel
    {
        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "Available")]
        public bool Available { get; set; } = true;

        [Display(Name = "Token")]
        public string? Token { get; set; }
    }
}
