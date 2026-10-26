namespace TiendaBolsos.Core.Modules.Inventory.DTOs
{
    public class CreateInventoryDto
    {
        public int IdProducto { get; set; }

        public int Stock { get; set; }

        public int MinStock { get; set; } = 5;
    }
}
