using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using TiendaBolsos.Infrastructure.Data;

using TiendaBolsos.Core.Modules.Products.Models;

using TiendaBolsos.Core.Modules.Products.Interfaces;

namespace TiendaBolsos.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductRepository(
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Productos
                .Include(x => x.Categoria)
                .Include(x => x.Marca)
                .ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Productos
                .Include(x => x.Categoria)
                .Include(x => x.Marca)
                .FirstOrDefaultAsync(x => x.IdProducto == id);
        }

        public async Task<Product> CreateAsync(Product product)
        {
            _context.CurrentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            _context.Productos.Add(product);

            await _context.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateAsync(Product product)
        {
            _context.CurrentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            _context.Productos.Update(product);

            await _context.SaveChangesAsync();

            return product;
        }
    }
}
