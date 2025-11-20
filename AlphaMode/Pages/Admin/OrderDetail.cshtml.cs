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
            public decimal TotalPrice { get; set; }
            public string? PromoCode { get; set; }
            public DateTime CreatedUtc { get; set; }
            public DateTime? UpdatedUtc { get; set; }

            [Display(Name = "Метод на доставка")]
            public DeliveryMethod DeliveryMethod { get; set; }

            // editable
            [Display(Name = "Статус")]
            public OrderStatus Status { get; set; }

            [Display(Name = "Бележки"), StringLength(500)]
            public string? Notes { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var o = await _db.Orders
                .AsNoTracking()
                .Include(x => x.Bundle)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (o == null) return RedirectToPage("/Admin/Orders");

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
            if (!ModelState.IsValid)
                return Page();

            var o = await _db.Orders.FirstOrDefaultAsync(x => x.Id == Form.Id);
            if (o == null)
            {
                TempData["Error"] = "Поръчката не бе намерена.";
                return RedirectToPage("/Admin/Orders");
            }

            // Update only editable fields
            o.Status = Form.Status;
            o.Notes = Form.Notes;
            o.UpdatedUtc = DateTime.UtcNow;
            o.DeliveryMethod = Form.DeliveryMethod;
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Поръчка #{o.Id} е обновена.";
            return RedirectToPage(new { id = o.Id }); // stay on detail
        }
    }
}