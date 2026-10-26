using TiendaBolsos.Core.Modules.Products.DTOs;

namespace TiendaBolsos.Core.Modules.Products.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetAllAsync();

        Task<ProductResponseDto?> GetByIdAsync(int id);

        Task<ProductResponseDto> CreateAsync(CreateProductDto dto);

        Task<ProductResponseDto> UpdateAsync(int id, UpdateProductDto dto);
    }
}
