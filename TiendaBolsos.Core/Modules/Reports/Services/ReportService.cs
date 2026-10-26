using TiendaBolsos.Core.Modules.Reports.DTOs;
using TiendaBolsos.Core.Modules.Reports.Interfaces;

namespace TiendaBolsos.Core.Modules.Reports.Services
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _repository;

        public ReportService(IReportRepository repository)
        {
            _repository = repository;
        }

        public async Task<KpiReportDto> GetKpisAsync()
        {
            var totalVentas = await _repository.GetTotalSalesAsync();
            var totalPedidos = await _repository.GetTotalOrdersAsync();

            return new KpiReportDto
            {
                TotalVentas = totalVentas,
                TotalPedidos = totalPedidos,
                TicketPromedio = totalPedidos > 0 ? totalVentas / totalPedidos : 0m,
                StockDisponible = await _repository.GetTotalStockAvailableAsync(),
                TotalProductos = await _repository.GetTotalProductsAsync(),
                ProductosVendidos = await _repository.GetTotalProductsSoldAsync(),
                LowStockCount = await _repository.GetLowStockCountAsync()
            };
        }

        public Task<IEnumerable<DailySalesDto>> GetDailySalesAsync()
        {
            return _repository.GetDailySalesAsync();
        }

        public Task<IEnumerable<TopProductsDto>> GetTopProductsAsync()
        {
            return _repository.GetTopProductsAsync();
        }

        public Task<IEnumerable<SalesByCategoryDto>> GetSalesByCategoryAsync()
        {
            return _repository.GetSalesByCategoryAsync();
        }

        public Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync()
        {
            return _repository.GetInventoryStatusAsync();
        }

        public Task<IEnumerable<TopProductsDto>> GetMostSoldProductsAsync()
        {
            return _repository.GetTopProductsAsync();
        }
    }
}
