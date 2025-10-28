using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Dashboard.Data;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.RpaSettings;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Common.Models.RpaSetting RpaSetting { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var setting = await _context.RpaSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound();
        }

        RpaSetting = setting;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var setting = await _context.RpaSettings.FindAsync(id);

        if (setting != null)
        {
            RpaSetting = setting;
            _context.RpaSettings.Remove(setting);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Setting '{setting.Key}' deleted successfully.";
        }

        return RedirectToPage("./Index");
    }
}
