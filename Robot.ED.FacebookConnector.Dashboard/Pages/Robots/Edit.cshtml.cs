using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Robots;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly AppDbContext _context;

    public EditModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var robot = await _context.Robots.FindAsync(id);

        if (robot == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = robot.Id,
            Name = robot.Name,
            Url = robot.Url,
            Available = robot.Available,
            Token = robot.Token
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var robot = await _context.Robots.FindAsync(Input.Id);

        if (robot == null)
        {
            return NotFound();
        }

        robot.Name = Input.Name;
        robot.Url = Input.Url;
        robot.Available = Input.Available;
        robot.Token = Input.Token;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RobotExists(Input.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["SuccessMessage"] = $"Robot '{Input.Name}' updated successfully.";
        return RedirectToPage("./Index");
    }

    private async Task<bool> RobotExists(int id)
    {
        return await _context.Robots.AnyAsync(e => e.Id == id);
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Display(Name = "URL")]
        [Url(ErrorMessage = "Please enter a valid URL")]
        public string Url { get; set; } = string.Empty;

        [Display(Name = "Available")]
        public bool Available { get; set; }

        [Display(Name = "Token")]
        public string? Token { get; set; }
    }
}
