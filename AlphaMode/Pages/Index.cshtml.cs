using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _db;
        public List<Bundle> FeaturedBundles { get; set; } = new();

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task OnGetAsync()
        {
            FeaturedBundles = await _db.Bundles
                .AsNoTracking()
                .Where(b => b.IsActive && b.FrontDisplay)
                .OrderBy(b => b.Price)
                .ToListAsync();
        }
    }
}
