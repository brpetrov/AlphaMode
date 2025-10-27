using AlphaMode.Data;
using AlphaMode.Models;
using AlphaMode.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace AlphaMode.Pages
{
    public class OrderPageModel : PageModel
    {
        private readonly IOrderService _orders;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _email;    
        private readonly ILogger<OrderPageModel> _log;

        [TempData] public string? OrderSummary { get; set; }

        public OrderPageModel(IOrderService orders, ApplicationDbContext db, IEmailSender email, ILogger<OrderPageModel> log)
        {
            _orders = orders;
            _db = db;
            _email = email;                   
            _log = log;
        }

        [BindProperty]
        public Order Order { get; set; } = new();

        [BindProperty]
        public decimal PromoPercent { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedBundleId { get; set; }

        // List of available bundles (for dropdown)
        public List<Bundle> Bundles { get; set; } = new();

        public async Task OnGetAsync(int? bundleId)
        {
            Bundles = await _db.Bundles
                .Where(b => b.IsActive)
                .OrderBy(b => b.Size)
                .ToListAsync();

            if (bundleId.HasValue)
            {
                var exists = Bundles.Any(b => b.Id == bundleId.Value);
                if (exists)
                {
                    Order.BundleId = bundleId.Value;
                }
            }
        }

        public async Task<IActionResult> OnGetPromoAsync(string code, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(code))
                return new JsonResult(new { valid = false, message = "Въведете промо код." });

            var promo = await _db.PromoCodes
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Code == code, ct);

            if (promo == null || !promo.IsActive || promo.ValidUntilUtc < DateTime.UtcNow)
                return new JsonResult(new { valid = false, message = "Промо кодът е невалиден или изтекъл." });

            var percent = promo.DiscountPercent;
            return new JsonResult(new { valid = true, percent });
        }

        public async Task<IActionResult> OnPostAsync(CancellationToken ct)
        {
            try
            {
                Bundles = await _db.Bundles
                    .Where(b => b.IsActive && (b.Size == 1 || b.Size == 2 || b.Size == 3))
                    .OrderBy(b => b.Size)
                    .ToListAsync(ct);

                if (!ModelState.IsValid)
                    return Page();

                var bundle = await _db.Bundles.FindAsync(new object?[] { Order.BundleId }, ct);
                if (bundle == null || !bundle.IsActive)
                {
                    ModelState.AddModelError("Order.BundleId", "Избраният пакет не е валиден.");
                    return Page();
                }

                var basePrice = bundle.Price;

                PromoPercent = 0m;
                if (!string.IsNullOrWhiteSpace(Order.PromoCode))
                {
                    var promo = await _db.PromoCodes
                        .AsNoTracking()
                        .FirstOrDefaultAsync(p => p.Code == Order.PromoCode, ct);

                    if (promo != null && promo.IsActive && promo.ValidUntilUtc >= DateTime.UtcNow)
                    {
                        PromoPercent = Convert.ToDecimal(promo.DiscountPercent);
                        if (PromoPercent < 0m) PromoPercent = 0m;
                        if (PromoPercent > 100m) PromoPercent = 100m;
                    }
                    else
                    {
                        ModelState.AddModelError("Order.PromoCode", "Промо кодът е невалиден или изтекъл.");
                    }
                }

                // If the form is invalid, return the page WITH PromoPercent set
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                decimal discountPercent = PromoPercent;

                // Final price
                var discountAmount = Math.Round(basePrice * (discountPercent / 100m), 2, MidpointRounding.AwayFromZero);
                var finalTotal = Math.Max(0m, Math.Round(basePrice - discountAmount, 2, MidpointRounding.AwayFromZero));

                // Persist final price
                Order.TotalPrice = finalTotal;

                // Save order
                var id = await _orders.CreateAsync(Order, ct);

                // Build success view (for TempData + page)
                var vm = new OrderSuccessView
                {
                    OrderId = id,
                    FullName = Order.FullName,
                    Telephone = Order.Telephone,
                    Email = Order.EmailAddress,
                    Address = Order.Address,
                    BundleName = bundle.Name,
                    BasePrice = basePrice,
                    DiscountPercent = discountPercent,
                    DiscountAmount = discountAmount,
                    FinalTotal = finalTotal,
                    PromoCode = string.IsNullOrWhiteSpace(Order.PromoCode) ? null : Order.PromoCode,
                    CreatedLocal = DateTime.UtcNow.ToLocalTime()
                };

                // Email (non-blocking)
                try
                {
                    var hasDiscount = discountPercent > 0m;
                    var orderDetailUrl = Url.Page(
                        pageName: "/Admin/OrderDetail",
                        pageHandler: null,
                        values: new { id },
                        protocol: Request.Scheme,
                        host: Request.Host.ToString(),
                        fragment: null
                    );
                    var promoDisplay = vm.PromoCode ?? "-";

                    var html = $@"
                                <!DOCTYPE html>
                                <html lang=""bg"">
                                <head>
                                  <meta charset=""utf-8"">
                                  <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
                                </head>
                                <body style=""margin:0;background:#f6f7fb;padding:24px;font-family:Segoe UI,Roboto,Helvetica,Arial,sans-serif;color:#111;"">
                                  <div style=""max-width:640px;margin:0 auto;background:#ffffff;border-radius:12px;box-shadow:0 2px 12px rgba(17,24,39,.08);overflow:hidden;"">
                                    <div style=""background:#111827;color:#ffffff;padding:16px 20px;"">
                                      <h2 style=""margin:0;font-size:18px;font-weight:600;"">AlphaMode — Нова поръчка #{id}</h2>
                                      <div style=""opacity:.8;font-size:12px;margin-top:2px;"">{vm.CreatedLocal:dd.MM.yyyy HH:mm}</div>
                                    </div>

                                    <div style=""padding:20px 20px 10px 20px;"">
                                      <h3 style=""margin:0 0 10px 0;font-size:16px;font-weight:600;"">Данни за клиента</h3>
                                      <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:100%;border-collapse:collapse;font-size:14px;"">
                                        <tr><td style=""padding:6px 0;width:160px;color:#6b7280;"">Име</td><td style=""padding:6px 0;"">{vm.FullName}</td></tr>
                                        <tr><td style=""padding:6px 0;color:#6b7280;"">Телефон</td><td style=""padding:6px 0;"">{vm.Telephone}</td></tr>
                                        <tr><td style=""padding:6px 0;color:#6b7280;"">Е=mail</td><td style=""padding:6px 0;"">{vm.Email}</td></tr>
                                        <tr><td style=""padding:6px 0;color:#6b7280;"">Адрес</td><td style=""padding:6px 0;"">{vm.Address}</td></tr>
                                        <tr><td style=""padding:6px 0;color:#6b7280;"">Промо код</td><td style=""padding:6px 0;"">{promoDisplay}</td></tr>
                                      </table>
                                    </div>

                                    <div style=""padding:10px 20px 20px 20px;"">
                                      <h3 style=""margin:10px 0;font-size:16px;font-weight:600;"">Поръчка</h3>
                                      <table role=""presentation"" cellspacing=""0"" cellpadding=""0"" border=""0"" style=""width:100%;border-collapse:collapse;font-size:14px;"">
                                        <tr style=""border-bottom:1px solid #eee"">
                                          <td style=""padding:8px 0;color:#6b7280;"">Пакет</td>
                                          <td style=""padding:8px 0;text-align:right;"">{vm.BundleName}</td>
                                        </tr>
                                        <tr style=""border-bottom:1px solid #eee"">
                                          <td style=""padding:8px 0;color:#6b7280;"">Цена (база)</td>
                                          <td style=""padding:8px 0;text-align:right;"">{vm.BasePrice:0.00} лв</td>
                                        </tr>
                                        {(hasDiscount ? $@"
                                        <tr style=""border-bottom:1px solid #eee"">
                                          <td style=""padding:8px 0;color:#059669;font-weight:600;"">Отстъпка ({vm.DiscountPercent:0.##}%)</td>
                                          <td style=""padding:8px 0;text-align:right;color:#059669;font-weight:600;"">- {vm.DiscountAmount:0.00} лв</td>
                                        </tr>" : "")}
                                        <tr>
                                          <td style=""padding:12px 0;font-size:16px;font-weight:700;"">Крайна цена</td>
                                          <td style=""padding:12px 0;text-align:right;font-size:16px;font-weight:700;"">{vm.FinalTotal:0.00} лв</td>
                                        </tr>
                                      </table>
                                    </div>

                                    <!-- Admin CTA -->
                                    <div style=""padding:20px;text-align:center;"">
                                        <a href=""{orderDetailUrl}""
                                            style=""display:inline-block;padding:12px 18px;border-radius:8px;background:#111827;color:#ffffff;text-decoration:none;font-weight:600;"">
                                        Отвори поръчката в администрация
                                        </a>
                                        <div style=""margin-top:8px;font-size:12px;color:#6b7280;"">
                                        Или копирайте линка: <br/>
                                        <span style=""word-break:break-all;color:#374151;"">{orderDetailUrl}</span>
                                        </div>
                                    </div>

                                    <div style=""background:#f9fafb;color:#6b7280;padding:14px 20px;font-size:12px"">
                                      Този имейл е генериран автоматично от AlphaMode уебсайта при нова поръчка.
                                    </div>
                                  </div>
                                </body>
                                </html>";

                    await _email.SendAsync(
                        subject: $"AlphaMode — Нова поръчка #{id}",
                        htmlBody: html,
                        toAddress: "alphamodebusiness@gmail.com"
                    );
                }
                catch (Exception ex)
                {
                    _log.LogError(ex, "Failed to send order email for order {OrderId}", id);
                }

                // Put one-time summary in TempData and redirect WITHOUT id
                OrderSummary = JsonSerializer.Serialize(vm);
                return RedirectToPage("/OrderSuccess");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "Възникна грешка при обработката на поръчката. Моля, опитайте отново.");
                return Page();
            }
        }
    }


}
