namespace TiendaBolsos.Core.Modules.Inventory.DTOs
{
    public class InventoryMovementDto
    {
        public int IdMovement { get; set; }

        public int IdInventory { get; set; }

        public string Type { get; set; } = string.Empty;

        public int Quantity { get; set; }

        public DateTime Date { get; set; }
    }
}
