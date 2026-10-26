namespace TiendaBolsos.Core.Modules.Reports.DTOs
{
    public class SalesByCategoryDto
    {
        public string Categoria { get; set; } = string.Empty;

        public decimal TotalVentas { get; set; }
    }
}
