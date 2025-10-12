using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Pages.Admin
{
    [Authorize(Roles = "Admin")]   // optional
    public class PromoCodesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public PromoCodesModel(ApplicationDbContext db) => _db = db;

        public IList<PromoCode> Codes { get; set; } = new List<PromoCode>();

        // form input
        [BindProperty]
        public PromoCodeInput Input { get; set; } = new();

        public class PromoCodeInput
        {
            [Required, StringLength(30)]
            public string Code { get; set; } = default!;

            [Required]
            public int Percentage { get; set; } = 10;

            [Display(Name = "Валиден до"), DataType(DataType.Date)]
            public DateTime? ValidUntil { get; set; }
        }

        public async Task OnGetAsync()
        {
            Codes = await _db.PromoCodes
                .OrderByDescending(p => p.CreatedUtc)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await OnGetAsync();
                return Page();
            }

            var code = Input.Code.Trim().ToUpperInvariant();

            if (await _db.PromoCodes.AnyAsync(p => p.Code == code))
            {
                ModelState.AddModelError(string.Empty, "Кодът вече съществува.");
                await OnGetAsync();
                return Page();
            }

            _db.PromoCodes.Add(new PromoCode
            {
                Code = code,
                DiscountPercent= Input.Percentage,
                ValidUntilUtc = Input.ValidUntil?.Date,   // store midnight UTC
                IsActive = true
            });

            await _db.SaveChangesAsync();
            return RedirectToPage();   // PRG pattern
        }
    }
}