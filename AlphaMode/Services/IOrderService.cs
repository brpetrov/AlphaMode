using AlphaMode.Models;

namespace AlphaMode.Services
{
    public interface IOrderService
    {
        Task<int> CreateAsync(Order order, CancellationToken ct = default);
        Task<Order?> GetAsync(int id, CancellationToken ct = default);
    }
}