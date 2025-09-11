using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AlphaMode.Pages
{
    public class OrderSuccessModel : PageModel
    {
        public int OrderId { get; set; }
        public void OnGet(int id) => OrderId = id;
    }
}
