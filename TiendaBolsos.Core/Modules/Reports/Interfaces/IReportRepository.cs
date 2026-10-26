using TiendaBolsos.Core.Modules.Reports.DTOs;

namespace TiendaBolsos.Core.Modules.Reports.Interfaces
{
    public interface IReportRepository
    {
        Task<decimal> GetTotalSalesAsync();

        Task<int> GetTotalOrdersAsync();

        Task<int> GetTotalProductsAsync();

        Task<int> GetTotalStockAvailableAsync();

        Task<int> GetTotalProductsSoldAsync();

        Task<int> GetLowStockCountAsync();

        Task<IEnumerable<DailySalesDto>> GetDailySalesAsync();

        Task<IEnumerable<TopProductsDto>> GetTopProductsAsync();

        Task<IEnumerable<SalesByCategoryDto>> GetSalesByCategoryAsync();

        Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync();
    }
}
