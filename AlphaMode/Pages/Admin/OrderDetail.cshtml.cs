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
    public class OrderDetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public OrderDetailModel(ApplicationDbContext db) => _db = db;

        [BindProperty]
        public OrderEditDto Form { get; set; } = new();

        // For package selection
        public List<Bundle> Bundles { get; set; } = new();

        public class OrderEditDto
        {
            public int Id { get; set; }

            // read-only display (populated for view)
            public string FullName { get; set; } = "";
            public string Address { get; set; } = "";
            public string Town { get; set; } = "";
            public string Email { get; set; } = "";
            public string Telephone { get; set; } = "";
            public DateTime? DateOfBirth { get; set; }
            public Bundle? Bundle { get; set; }

            [Display(Name = "Крайна цена")]
            public decimal TotalPrice { get; set; }   // display only, not edited

            [Display(Name = "Промо код")]
            public string? PromoCode { get; set; }

            public DateTime CreatedUtc { get; set; }
            public DateTime? UpdatedUtc { get; set; }

            [Display(Name = "Метод на доставка")]
            public DeliveryMethod DeliveryMethod { get; set; }

            [Display(Name = "Статус")]
            public OrderStatus Status { get; set; }

            [Display(Name = "Бележки"), StringLength(500)]
            public string? Notes { get; set; }

            [Display(Name = "Пакет")]
            public int BundleId { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var o = await _db.Orders
                .AsNoTracking()
                .Include(x => x.Bundle)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (o == null)
            {
                TempData["Error"] = "Поръчката не бе намерена.";
                return RedirectToPage("/Admin/Orders");
            }

            // Load bundles for dropdown
            Bundles = await _db.Bundles
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Price)
                .ThenBy(b => b.Name)
                .ToListAsync();

            Form = new OrderEditDto
            {
                Id = o.Id,
                FullName = o.FullName,
                Address = o.Address,
                Town = o.Town,
                Email = o.EmailAddress,
                Telephone = o.Telephone,
                DateOfBirth = o.DateOfBirth,
                Bundle = o.Bundle,
                BundleId = o.BundleId,         // assumes Order has BundleId FK
                TotalPrice = o.TotalPrice,
                PromoCode = o.PromoCode,
                CreatedUtc = o.CreatedUtc,
                UpdatedUtc = o.UpdatedUtc,
                DeliveryMethod = o.DeliveryMethod,
                Status = o.Status,
                Notes = o.Notes
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Bundles are needed for redisplay if validation fails
            Bundles = await _db.Bundles
                .AsNoTracking()
                .Where(b => b.IsActive)
                .OrderBy(b => b.Price)
                .ThenBy(b => b.Name)
                .ToListAsync();

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var o = await _db.Orders.FirstOrDefaultAsync(x => x.Id == Form.Id);
            if (o == null)
            {
                TempData["Error"] = "Поръчката не бе намерена.";
                return RedirectToPage("/Admin/Orders");
            }

            // Normalize promo code
            var promoInput = Form.PromoCode?.Trim();
            PromoCode? promo = null;

            if (!string.IsNullOrEmpty(promoInput))
            {
                var nowUtc = DateTime.UtcNow;

                promo = await _db.PromoCodes
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p =>
                        p.Code == promoInput &&
                        p.IsActive &&
                        (p.ValidUntilUtc == null || p.ValidUntilUtc >= nowUtc));

                if (promo == null)
                {
                    ModelState.AddModelError("Form.PromoCode", "Невалиден или изтекъл промо код.");
                    // Keep displayed TotalPrice as existing order's value
                    Form.TotalPrice = o.TotalPrice;
                    return Page();
                }
            }

            // Compute base price from selected bundle
            decimal basePrice = 0m;
            Bundle? selectedBundle = null;

            if (Form.BundleId != null)
            {
                selectedBundle = await _db.Bundles
                    .AsNoTracking()
                    .FirstOrDefaultAsync(b => b.Id == Form.BundleId && b.IsActive);

                if (selectedBundle == null)
                {
                    ModelState.AddModelError("Form.BundleId", "Избраният пакет не съществува или не е активен.");
                    Form.TotalPrice = o.TotalPrice;
                    return Page();
                }

                basePrice = selectedBundle.Price;
            }

            // Discount
            int discountPercent = promo?.DiscountPercent ?? 0;
            if (discountPercent < 0) discountPercent = 0;
            if (discountPercent > 100) discountPercent = 100;

            var discountFactor = (100m - discountPercent) / 100m;
            var finalPrice = Math.Round(basePrice * discountFactor, 2, MidpointRounding.AwayFromZero);

            // Update editable fields
            o.Status = Form.Status;
            o.Notes = Form.Notes;
            o.UpdatedUtc = DateTime.UtcNow;
            o.DeliveryMethod = Form.DeliveryMethod;

            o.BundleId = Form.BundleId;
            o.TotalPrice = finalPrice;
            o.PromoCode = promoInput;   // store typed code (or null/empty)

            await _db.SaveChangesAsync();

            TempData["Success"] = $"Поръчка #{o.Id} е обновена.";

            // After save, redirect to GET so we see recalculated price and bundle nicely
            return RedirectToPage(new { id = o.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var o = await _db.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (o == null)
            {
                TempData["Error"] = "Поръчката не бе намерена.";
                return RedirectToPage("/Admin/Orders");
            }

            _db.Orders.Remove(o);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Поръчка #{id} е изтрита.";
            return RedirectToPage("/Admin/Orders");
        }
    }
}
