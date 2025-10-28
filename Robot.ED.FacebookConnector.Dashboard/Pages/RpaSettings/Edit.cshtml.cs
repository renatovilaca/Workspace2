using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Robot.ED.FacebookConnector.Common.Configuration;

namespace Robot.ED.FacebookConnector.Dashboard.Pages.RpaSettings;

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

        var setting = await _context.RpaSettings.FindAsync(id);

        if (setting == null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = setting.Id,
            Key = setting.Key,
            Value = setting.Value
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var setting = await _context.RpaSettings.FindAsync(Input.Id);

        if (setting == null)
        {
            return NotFound();
        }

        setting.Key = Input.Key;
        setting.Value = Input.Value;
        setting.UpdatedAt = DateTime.UtcNow;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RpaSettingExists(Input.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        TempData["SuccessMessage"] = $"Setting '{Input.Key}' updated successfully.";
        return RedirectToPage("./Index");
    }

    private async Task<bool> RpaSettingExists(int id)
    {
        return await _context.RpaSettings.AnyAsync(e => e.Id == id);
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        [Display(Name = "Key")]
        public string Key { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Value")]
        public string Value { get; set; } = string.Empty;
    }
}
