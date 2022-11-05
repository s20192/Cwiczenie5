using System.ComponentModel.DataAnnotations;

namespace WarerhouseApp.Model
{
    public class Product
    {
        [Required]
        public int IdProduct { get; set; }

        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

    }
}
