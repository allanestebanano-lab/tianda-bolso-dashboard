using TiendaBolsos.Core.Modules.Products.Models;

namespace TiendaBolsos.Core.Modules.Products.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync();

        Task<Product?> GetByIdAsync(int id);

        Task<Product> CreateAsync(Product product);

        Task<Product> UpdateAsync(Product product);
    }
}
