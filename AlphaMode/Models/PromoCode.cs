using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Models
{
    public class PromoCode
    {
        public int Id { get; set; }

        [Required, StringLength(30)]
        public string Code { get; set; } = default!;   

        public bool IsActive { get; set; } = true;
        public DateTime? ValidUntilUtc { get; set; }
        public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    }
}
