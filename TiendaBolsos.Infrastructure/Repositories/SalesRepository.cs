using Microsoft.EntityFrameworkCore;

using TiendaBolsos.Core.Modules.Sales.Interfaces;
using TiendaBolsos.Core.Modules.Sales.Models;
using TiendaBolsos.Infrastructure.Data;

namespace TiendaBolsos.Infrastructure.Repositories
{
    public class SalesRepository : ISalesRepository
    {
        private readonly AppDbContext _context;

        public SalesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Venta>> GetAllAsync()
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .ToListAsync();
        }

        public async Task<Venta?> GetByIdAsync(int id)
        {
            return await _context.Ventas
                .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.IdVenta == id);
        }

        public async Task<Venta> CreateAsync(Venta venta)
        {
            _context.Ventas.Add(venta);

            await _context.SaveChangesAsync();

            return venta;
        }
    }
}
