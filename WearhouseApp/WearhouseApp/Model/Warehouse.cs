using System.ComponentModel.DataAnnotations;

namespace WarehouseApp.Model
{
    public class Warehouse
    {
        [Required]
        public int IdWarehouse { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
