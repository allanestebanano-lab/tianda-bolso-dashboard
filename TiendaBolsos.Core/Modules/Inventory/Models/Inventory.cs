using TiendaBolsos.Core.Modules.Products.Models;

namespace TiendaBolsos.Core.Modules.Inventory.Models
{
    public class Inventory
    {
        public int IdInventory { get; set; }

        public int IdProducto { get; set; }

        public int Stock { get; set; }

        public int MinStock { get; set; }

        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        public Product? Producto { get; set; }
    }
}
