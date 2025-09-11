using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Models
{
    public enum OrderStatus { New = 0, Processing = 1, Completed = 2, Cancelled = 3 }

    public class Order
    {
        public int Id { get; set; }

        // Customer
        [Required, Display(Name = "Име и фамилия"), StringLength(80)]
        public string FullName { get; set; } = default!;

        [Required, Display(Name = "Адрес"), StringLength(200)]
        public string Address { get; set; } = default!;

        [Required, Display(Name = "Телефон"), Phone, StringLength(30)]
        public string Telephone { get; set; } = default!;

        [Display(Name = "Дата на раждане"), DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        // Order
        [Required, Display(Name = "Количество"), Range(1, 24)]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Промо код"), StringLength(30)]
        public string? PromoCode { get; set; }

        // System
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }
    }
}
