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
    public class BundleDetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public BundleDetailModel(ApplicationDbContext db) => _db = db;

        public bool IsCreate => (Input?.Id ?? 0) == 0;

        [BindProperty]
        public BundleInput Input { get; set; } = new();

        public class BundleInput
        {
            public int Id { get; set; }

            [Display(Name = "Име"), Required, StringLength(40)]
            public string Name { get; set; } = "";

            [Display(Name = "Брой"), Range(1, 100)]
            public int Size { get; set; }

            [Display(Name = "Цена (лв.)"), Range(typeof(decimal), "0", "9999")]
            public decimal Price { get; set; }

            [Display(Name = "Активен")]
            public bool IsActive { get; set; } = true;

            [Display(Name = "Показвай на началната страница")]
            public bool FrontDisplay { get; set; } = false;
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null || id == 0)
            {
                // Create mode defaults
                Input = new BundleInput { IsActive = true, FrontDisplay = false };
                return Page();
            }

            var b = await _db.Bundles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id.Value);
            if (b == null) return RedirectToPage("/Admin/Bundles");

            Input = new BundleInput
            {
                Id = b.Id,
                Name = b.Name,
                Size = b.Size,
                Price = b.Price,
                IsActive = b.IsActive,
                FrontDisplay = b.FrontDisplay
            };
            return Page();
        }

        public async Task<IActionResult> OnPostSaveAsync()
        {
            if (!ModelState.IsValid) return Page();

            if (Input.Id == 0)
            {
                // Create
                var entity = new Bundle
                {
                    Name = Input.Name.Trim(),
                    Size = Input.Size,
                    Price = Input.Price,
                    IsActive = Input.IsActive,
                    FrontDisplay = Input.FrontDisplay
                };
                _db.Bundles.Add(entity);
                await _db.SaveChangesAsync();
                return RedirectToPage("/Admin/Bundles");
            }
            else
            {
                // Update
                var entity = await _db.Bundles.FindAsync(Input.Id);
                if (entity == null)
                {
                    ModelState.AddModelError(string.Empty, "Пакетът не бе намерен.");
                    return Page();
                }

                entity.Name = Input.Name.Trim();
                entity.Size = Input.Size;
                entity.Price = Input.Price;
                entity.IsActive = Input.IsActive;
                entity.FrontDisplay = Input.FrontDisplay;

                await _db.SaveChangesAsync();
                return RedirectToPage("/Admin/Bundles");
            }
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var entity = await _db.Bundles.FindAsync(id);
            if (entity == null)
            {
                ModelState.AddModelError(string.Empty, "Пакетът не бе намерен.");
                return Page();
            }

            try
            {
                _db.Bundles.Remove(entity);
                await _db.SaveChangesAsync();
                return RedirectToPage("/Admin/Bundles");
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Пакетът не може да бъде изтрит, защото се използва.");
                return Page();
            }
        }
    }
}
