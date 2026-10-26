namespace TiendaBolsos.Core.Modules.Sales.DTOs
{
    public class CreateSaleItemDto
    {
        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }
    }
}
