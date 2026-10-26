using TiendaBolsos.Core.Modules.Reports.DTOs;

namespace TiendaBolsos.Core.Modules.Reports.Interfaces
{
    public interface IReportService
    {
        Task<KpiReportDto> GetKpisAsync();

        Task<IEnumerable<DailySalesDto>> GetDailySalesAsync();

        Task<IEnumerable<TopProductsDto>> GetTopProductsAsync();

        Task<IEnumerable<SalesByCategoryDto>> GetSalesByCategoryAsync();

        Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync();

        Task<IEnumerable<TopProductsDto>> GetMostSoldProductsAsync();
    }
}
