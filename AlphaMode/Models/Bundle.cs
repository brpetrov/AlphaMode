using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Models
{
    public class Bundle
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Name { get; set; } = "";

        [Range(1, 100)]
        public int Size { get; set; } // Number of bottles

        [Range(0, 9999)]
        public decimal Price { get; set; } // Bundle price

        public bool IsActive { get; set; } = true;
    }
}
