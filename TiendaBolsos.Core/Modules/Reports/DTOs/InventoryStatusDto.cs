namespace TiendaBolsos.Core.Modules.Reports.DTOs
{
    public class InventoryStatusDto
    {
        public int IdProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public int Stock { get; set; }

        public int MinStock { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
