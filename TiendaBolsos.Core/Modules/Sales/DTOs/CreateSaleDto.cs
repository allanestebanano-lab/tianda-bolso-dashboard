namespace TiendaBolsos.Core.Modules.Sales.DTOs
{
    public class CreateSaleDto
    {
        public string? MetodoPago { get; set; }

        public List<CreateSaleItemDto> Detalles { get; set; } = new();
    }
}
