using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Models
{
    public enum OrderStatus
    {
        [Display(Name = "Нова")] New = 0,
        [Display(Name = "За доставка")] OutForDelivery = 1,
        [Display(Name = "Доставена")] Delivered = 2,
        [Display(Name = "Отказана")] Refused = 3,
        [Display(Name = "Върната")] Returned = 4,
        [Display(Name = "Анулирана")] Cancelled = 5
    }

    public enum DeliveryMethod
    {
        [Display(Name = "До адрес")] HomeAddress = 0,
        [Display(Name = "До офис на Еконт")] EcontOffice = 1
    }

    // Custom attribute: require at least MinYears old (inclusive)
    public sealed class MinAgeAttribute : ValidationAttribute
    {
        public int Years { get; }
        public MinAgeAttribute(int years) => Years = years;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is null) return ValidationResult.Success; // allow empty DOB
            if (value is not DateTime dob) return new ValidationResult("Невалидна дата.");

            var today = DateTime.UtcNow.Date; // use UTC consistently
            var eighteenth = dob.Date.AddYears(Years);
            return eighteenth <= today
                ? ValidationResult.Success
                : new ValidationResult($"Трябва да имате навършени {Years} години.");
        }
    }

    public class Order
    {
        public int Id { get; set; }

        // Customer
        [Required, Display(Name = "Име и фамилия"), StringLength(80)]
        public string FullName { get; set; } = default!;

        [Required, Display(Name = "Адрес"), StringLength(200)]
        public string Address { get; set; } = default!;

        [Required, Display(Name = "Населено Място (Град / Село)"), StringLength(80)]
        public string Town { get; set; } = default!;

        [Required, Display(Name = "Телефон"), Phone, StringLength(30)]
        public string Telephone { get; set; } = default!;

        [Required, Display(Name = "Доставка до")]
        public DeliveryMethod DeliveryMethod { get; set; } = DeliveryMethod.HomeAddress;

        [Required, Display(Name = "Имейл"), EmailAddress, StringLength(120)]
        public string EmailAddress { get; set; } = default!;

        [Display(Name = "Дата на раждане"), DataType(DataType.Date), MinAge(18)]
        public DateTime? DateOfBirth { get; set; }

        [Required, Display(Name = "Пакет")]
        public int BundleId { get; set; }

        [ValidateNever]
        public Bundle Bundle { get; set; } = default!;

        [Display(Name = "Крайна цена")]
        public decimal TotalPrice { get; set; }

        [Display(Name = "Промо код"), StringLength(30)]
        public string? PromoCode { get; set; }

        [Display(Name = "Бележки"), StringLength(500)]
        public string? Notes { get; set; }

        // System
        public OrderStatus Status { get; set; } = OrderStatus.New;
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedUtc { get; set; }

        //Security
        [MaxLength(36)] public string? DeviceId { get; set; }   // GUID from cookie
        [MaxLength(64)] public string? IpAddress { get; set; }   // remote IP
        [MaxLength(64)] public string? CustomerKey { get; set; }  // SHA-256(normalizedPhone|normalizedEmail)
    }
}
