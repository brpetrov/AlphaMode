using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class OrdersModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public OrdersModel(ApplicationDbContext db) => _db = db;

        public IList<Order> Orders { get; private set; } = new List<Order>();

        public async Task OnGetAsync()
        {
            Orders = await _db.Orders
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedUtc)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var order = await _db.Orders.FindAsync(id);
            if (order == null)
            {
                TempData["Error"] = "Поръчката не бе намерена.";
                return RedirectToPage();
            }

            _db.Orders.Remove(order);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Поръчка #{id} е изтрита.";
            return RedirectToPage();
        }
    }
}
