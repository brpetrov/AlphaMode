using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) => _db = db;

        public KpiView Kpi { get; set; } = new();
        public List<StatusBucket> StatusCounts { get; set; } = new();
        public List<Order> Recent { get; set; } = new();

        public class KpiView
        {
            public int TodayOrders { get; set; }
            public decimal MonthRevenue { get; set; }
            public int NewCount { get; set; }
            public int DeliveredThisMonth { get; set; }
        }

        public class StatusBucket
        {
            public OrderStatus Status { get; set; }
            public int Count { get; set; }
        }

        public static string StatusBadgeClass(OrderStatus status) => status switch
        {
            OrderStatus.New => "bg-primary",
            OrderStatus.OutForDelivery => "bg-warning",
            OrderStatus.Delivered => "bg-success",
            OrderStatus.Refused => "bg-danger",
            OrderStatus.Returned => "bg-secondary",
            OrderStatus.Cancelled => "bg-dark",
            _ => "bg-secondary"
        };

        public async Task OnGetAsync()
        {
            var nowLocal = DateTime.Now;
            var todayLocal = nowLocal.Date;
            var firstOfMonthLocal = new DateTime(nowLocal.Year, nowLocal.Month, 1);

            // Convert to UTC for comparisons against CreatedUtc
            var todayUtc = todayLocal.ToUniversalTime();
            var tomorrowUtc = todayLocal.AddDays(1).ToUniversalTime();
            var monthStartUtc = firstOfMonthLocal.ToUniversalTime();
            var nextMonthStartUtc = firstOfMonthLocal.AddMonths(1).ToUniversalTime();

            // KPIs
            Kpi.TodayOrders = await _db.Orders
                .CountAsync(o => o.CreatedUtc >= todayUtc && o.CreatedUtc < tomorrowUtc);

            Kpi.MonthRevenue = await _db.Orders
                .Where(o => o.CreatedUtc >= monthStartUtc && o.CreatedUtc < nextMonthStartUtc)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

            Kpi.NewCount = await _db.Orders.CountAsync(o => o.Status == OrderStatus.New);

            Kpi.DeliveredThisMonth = await _db.Orders.CountAsync(o =>
                o.Status == OrderStatus.Delivered &&
                o.CreatedUtc >= monthStartUtc && o.CreatedUtc < nextMonthStartUtc);

            // Status counts (last 30 days)
            var fromUtc = DateTime.UtcNow.AddDays(-30);
            StatusCounts = await _db.Orders
                .Where(o => o.CreatedUtc >= fromUtc)
                .GroupBy(o => o.Status)
                .Select(g => new StatusBucket { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            // Recent 10 orders
            Recent = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Bundle)
                .OrderByDescending(o => o.CreatedUtc)
                .Take(10)
                .ToListAsync();
        }
    }
}
