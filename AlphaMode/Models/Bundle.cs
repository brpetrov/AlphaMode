using System.ComponentModel.DataAnnotations;

namespace AlphaMode.Models
{
    public class Bundle
    {
        public int Id { get; set; }

        [Required, StringLength(40)]
        public string Name { get; set; } = "";

        [Range(1, 100)]
        public int Size { get; set; }

        [Range(typeof(decimal), "0", "9999")]
        public decimal Price { get; set; }

        public bool FrontDisplay { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }
}
