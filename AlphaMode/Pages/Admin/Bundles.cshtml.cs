using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class BundlesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public BundlesModel(ApplicationDbContext db) => _db = db;

        public IList<Bundle> Bundles { get; set; } = new List<Bundle>();

        public class BundleInput
        {
            [Required, StringLength(40)]
            public string Name { get; set; } = "";

            [Range(1, 100)]
            public int Size { get; set; }

            [Range(typeof(decimal), "0", "9999")]
            public decimal Price { get; set; }

            public bool IsActive { get; set; }
            public bool FrontDisplay { get; set; } = false;
        }

        public async Task OnGetAsync()
        {
            Bundles = await _db.Bundles
                .OrderBy(b => b.Size)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int deleteId)
        {
            var bundle = await _db.Bundles.FindAsync(deleteId);
            if (bundle == null)
            {
                ModelState.AddModelError(string.Empty, "Пакетът не бе намерен.");
                await OnGetAsync();
                return Page();
            }

            try
            {
                _db.Bundles.Remove(bundle);
                await _db.SaveChangesAsync();
                return RedirectToPage();
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Пакетът не може да бъде изтрит, защото се използва.");
                await OnGetAsync();
                return Page();
            }
        }

    }
}
