namespace TiendaBolsos.Core.Modules.Inventory.DTOs
{
    public class CreateInventoryMovementDto
    {
        public int IdInventory { get; set; }

        public string Type { get; set; } = string.Empty;

        public int Quantity { get; set; }
    }
}
