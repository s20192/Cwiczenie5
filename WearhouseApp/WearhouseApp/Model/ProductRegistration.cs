using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Model
{
    public class ProductRegistration
    {
        [Required]
        public int IdProduct { get; set; }

        [Required]
        public int IdWarehouse { get; set; }

        [Required]
        [Min(0)]
        public int Amount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
