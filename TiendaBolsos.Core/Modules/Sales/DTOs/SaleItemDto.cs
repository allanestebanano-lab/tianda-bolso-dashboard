namespace TiendaBolsos.Core.Modules.Sales.DTOs
{
    public class SaleItemDto
    {
        public int IdProducto { get; set; }

        public string? NombreProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }
    }
}
