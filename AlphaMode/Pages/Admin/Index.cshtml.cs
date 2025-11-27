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

        // selected month context
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }

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

        public async Task OnGetAsync(int? year, int? month)
        {
            var nowLocal = DateTime.Now;

            // determine selected month (query params; default = current month)
            SelectedYear = year ?? nowLocal.Year;
            SelectedMonth = month ?? nowLocal.Month;

            var monthStartLocal = new DateTime(SelectedYear, SelectedMonth, 1);
            var nextMonthStartLocal = monthStartLocal.AddMonths(1);

            // UTC ranges for CreatedUtc
            var monthStartUtc = monthStartLocal.ToUniversalTime();
            var nextMonthStartUtc = nextMonthStartLocal.ToUniversalTime();

            var todayLocal = nowLocal.Date;
            var todayUtc = todayLocal.ToUniversalTime();
            var tomorrowUtc = todayLocal.AddDays(1).ToUniversalTime();

            // 1) Orders created today (all statuses)
            Kpi.TodayOrders = await _db.Orders
                .CountAsync(o => o.CreatedUtc >= todayUtc && o.CreatedUtc < tomorrowUtc);

            // 2) Revenue for selected month – only Delivered
            Kpi.MonthRevenue = await _db.Orders
                .Where(o =>
                    o.Status == OrderStatus.Delivered &&
                    o.CreatedUtc >= monthStartUtc &&
                    o.CreatedUtc < nextMonthStartUtc)
                .SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

            // 3) New orders (global "to do" queue)
            Kpi.NewCount = await _db.Orders
                .CountAsync(o => o.Status == OrderStatus.New);

            // 4) Delivered orders in selected month
            Kpi.DeliveredThisMonth = await _db.Orders
                .CountAsync(o =>
                    o.Status == OrderStatus.Delivered &&
                    o.CreatedUtc >= monthStartUtc &&
                    o.CreatedUtc < nextMonthStartUtc);

            // Status counts for selected month
            var rawBuckets = await _db.Orders
                .Where(o => o.CreatedUtc >= monthStartUtc && o.CreatedUtc < nextMonthStartUtc)
                .GroupBy(o => o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            // ensure all statuses appear (even with 0)
            StatusCounts = Enum.GetValues<OrderStatus>()
                .Select(st => new StatusBucket
                {
                    Status = st,
                    Count = rawBuckets.FirstOrDefault(x => x.Status == st)?.Count ?? 0
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            // Recent 10 orders for selected month
            Recent = await _db.Orders
                .AsNoTracking()
                .Include(o => o.Bundle)
                .Where(o => o.CreatedUtc >= monthStartUtc && o.CreatedUtc < nextMonthStartUtc)
                .OrderByDescending(o => o.CreatedUtc)
                .Take(10)
                .ToListAsync();
        }
    }
}
