namespace TiendaBolsos.Core.Modules.Reports.DTOs
{
    public class KpiReportDto
    {
        public decimal TotalVentas { get; set; }

        public int TotalPedidos { get; set; }

        public decimal TicketPromedio { get; set; }

        public int StockDisponible { get; set; }

        public int TotalProductos { get; set; }

        public int ProductosVendidos { get; set; }

        public int LowStockCount { get; set; }
    }
}
