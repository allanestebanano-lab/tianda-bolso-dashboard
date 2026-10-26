namespace TiendaBolsos.Core.Modules.Inventory.DTOs
{
    public class InventoryResponseDto
    {
        public int IdInventory { get; set; }

        public int IdProducto { get; set; }

        public int Stock { get; set; }

        public int MinStock { get; set; }

        public DateTime LastUpdate { get; set; }

        public string? NombreProducto { get; set; }

        public string? CodigoProducto { get; set; }
    }
}
