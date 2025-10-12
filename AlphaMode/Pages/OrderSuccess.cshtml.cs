using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace AlphaMode.Pages
{
    public class OrderSuccessModel : PageModel
    {
        [TempData] public string? OrderSummary { get; set; }
        public OrderSuccessView? View { get; private set; }

        public void OnGet()
        {
            if (!string.IsNullOrEmpty(OrderSummary))
            {
                View = JsonSerializer.Deserialize<OrderSuccessView>(OrderSummary);
            }
        }
    }

    public class OrderSuccessView
    {
        public int OrderId { get; set; }
        public string FullName { get; set; } = "";
        public string Telephone { get; set; } = "";
        public string Address { get; set; } = "";
        public string BundleName { get; set; } = "";
        public decimal BasePrice { get; set; }
        public decimal DiscountPercent { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalTotal { get; set; }
        public string? PromoCode { get; set; }
        public DateTime CreatedLocal { get; set; }
    }
}
