using AlphaMode.Models;
using AlphaMode.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Pages
{
    public class OrderPageModel : PageModel
    {
        private readonly IOrderService _orders;
        public OrderPageModel(IOrderService orders) => _orders = orders;

        [BindProperty]
        public Order Order { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return Page();

            var id = await _orders.CreateAsync(Order, ct);
            return RedirectToPage("/OrderSuccess", new { id });
        }
    }
}
