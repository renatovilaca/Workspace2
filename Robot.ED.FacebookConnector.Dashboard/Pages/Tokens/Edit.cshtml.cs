using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Dashboard.Data;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.Tokens;

[Authorize(Roles = "Admin")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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

        var token = await _context.Tokens.FindAsync(id);

        if (token == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = token.Id,
            UserName = token.UserName,
            TokenValue = token.TokenValue
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var token = await _context.Tokens.FindAsync(Input.Id);

        if (token == null)
        {
            return NotFound();
        }

        token.UserName = Input.UserName;
        token.TokenValue = Input.TokenValue;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TokenExists(Input.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["SuccessMessage"] = $"Token for '{Input.UserName}' updated successfully.";
        return RedirectToPage("./Index");
    }

    private async Task<bool> TokenExists(int id)
    {
        return await _context.Tokens.AnyAsync(e => e.Id == id);
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Token Value")]
        public string TokenValue { get; set; } = string.Empty;
    }
}
