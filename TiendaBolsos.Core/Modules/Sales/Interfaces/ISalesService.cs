using TiendaBolsos.Core.Modules.Sales.DTOs;

namespace TiendaBolsos.Core.Modules.Sales.Interfaces
{
    public interface ISalesService
    {
        Task<IEnumerable<SaleResponseDto>> GetAllAsync();

        Task<SaleResponseDto?> GetByIdAsync(int id);

        Task<SaleResponseDto> CreateAsync(int userId, CreateSaleDto dto);
    }
}
