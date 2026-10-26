namespace TiendaBolsos.Core.Modules.Sales.DTOs
{
    public class SaleResponseDto
    {
        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; }

        public decimal Total { get; set; }

        public string? MetodoPago { get; set; }

        public int IdUsuario { get; set; }

        public List<SaleItemDto> Detalles { get; set; } = new();
    }
}
