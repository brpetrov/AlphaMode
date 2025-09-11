using AlphaMode.Data;
using AlphaMode.Models;
using Microsoft.EntityFrameworkCore;

namespace AlphaMode.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _db;
        public OrderService(ApplicationDbContext db) => _db = db;

        public async Task<int> CreateAsync(Order order, CancellationToken ct = default)
        {
            if (order.DateOfBirth is { } dob && dob > DateTime.Today)
                throw new InvalidOperationException("Дата на раждане не може да е в бъдещето.");

            order.FullName = order.FullName.Trim();
            order.Address = order.Address.Trim();
            order.Telephone = NormalizePhone(order.Telephone);
            if (!string.IsNullOrWhiteSpace(order.PromoCode))
            {
                var code = order.PromoCode.Trim().ToUpperInvariant();
                var now = DateTime.UtcNow;

                var exists = await _db.PromoCodes.AnyAsync(p =>
                    p.IsActive &&
                    p.Code == code &&
                    (p.ValidUntilUtc == null || p.ValidUntilUtc >= now), ct);

                if (!exists)
                    throw new InvalidOperationException("Невалиден или изтекъл промо код.");

                order.PromoCode = code;
            }

            order.Status = OrderStatus.New;
            order.CreatedUtc = DateTime.UtcNow;

            _db.Orders.Add(order);
            await _db.SaveChangesAsync(ct);
            return order.Id;
        }

        public Task<Order?> GetAsync(int id, CancellationToken ct = default) =>
            _db.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, ct);

        private static string NormalizePhone(string phone)
        {
            return new string(phone.Where(char.IsDigit).ToArray());
        }
    }
}
