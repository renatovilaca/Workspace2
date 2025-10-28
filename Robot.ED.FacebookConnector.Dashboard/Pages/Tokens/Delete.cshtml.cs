using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Robot.ED.FacebookConnector.Dashboard.Data;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Tokens;

[Authorize(Roles = "Admin")]
public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Common.Models.Token Token { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var token = await _context.Tokens.FindAsync(id);

        if (token == null)
        {
            return NotFound();
        }

        Token = token;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var token = await _context.Tokens.FindAsync(id);

        if (token != null)
        {
            Token = token;
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Token for '{token.UserName}' deleted successfully.";
        }

        return RedirectToPage("./Index");
    }
}
