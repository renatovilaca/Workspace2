using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Robots;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly AppDbContext _context;

    public DeleteModel(AppDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Common.Models.Robot Robot { get; set; } = null!;

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

        Robot = robot;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var robot = await _context.Robots.FindAsync(id);

        if (robot != null)
        {
            Robot = robot;
            _context.Robots.Remove(robot);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Robot '{robot.Name}' deleted successfully.";
        }

        return RedirectToPage("./Index");
    }
}
