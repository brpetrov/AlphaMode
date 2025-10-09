using AlphaMode.Data;
using AlphaMode.Models;
using AlphaMode.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Pages
{
    public class OrderPageModel : PageModel
    {
        private readonly IOrderService _orders;
        private readonly ApplicationDbContext _db;
        public OrderPageModel(IOrderService orders, ApplicationDbContext db)
        {
            _orders = orders;
            _db = db;
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        // List of available bundles (for dropdown)
        public List<Bundle> Bundles { get; set; } = new();

        public async Task OnGetAsync()
        {
            Bundles = await _db.Bundles
                .Where(b => b.IsActive && (b.Size == 1 || b.Size == 2 || b.Size == 3))
                .OrderBy(b => b.Size)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            try
            {
                // Reload bundles in case of form error
                Bundles = await _db.Bundles
                    .Where(b => b.IsActive && (b.Size == 1 || b.Size == 2 || b.Size == 3))
                    .OrderBy(b => b.Size)
                    .ToListAsync();

                if (!ModelState.IsValid)
                {
                    // For debugging:
                    var errors = ModelState.Where(x => x.Value.Errors.Count > 0)
                        .Select(x => $"{x.Key}: {x.Value.Errors[0].ErrorMessage}")
                        .ToList();
                    // Set a breakpoint here or log errors to understand which field(s) are failing.
                    return Page();
                }

                // Get bundle info and price from DB
                var bundle = await _db.Bundles.FindAsync(Order.BundleId);
                if (bundle == null || !bundle.IsActive)
                {
                    ModelState.AddModelError("Order.BundleId", "Избраният пакет не е валиден.");
                    return Page();
                }

                Order.TotalPrice = bundle.Price;

                // Save order as usual
                var id = await _orders.CreateAsync(Order, ct);
                return RedirectToPage("/OrderSuccess", new { id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Възникна грешка при обработката на поръчката. Моля, опитайте отново.");
                return Page();
                // Log the exception (ex) as needed
            }
           
        }
    }
}
