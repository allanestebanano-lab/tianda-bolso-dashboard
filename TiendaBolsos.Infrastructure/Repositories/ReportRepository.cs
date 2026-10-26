using Microsoft.EntityFrameworkCore;

using TiendaBolsos.Core.Modules.Reports.DTOs;
using TiendaBolsos.Core.Modules.Reports.Interfaces;
using TiendaBolsos.Infrastructure.Data;

namespace TiendaBolsos.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalSalesAsync()
        {
            return await _context.Ventas.SumAsync(v => (decimal?)v.Total) ?? 0m;
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _context.Ventas.CountAsync();
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _context.Productos.CountAsync();
        }

        public async Task<int> GetTotalStockAvailableAsync()
        {
            return await _context.Inventories.SumAsync(i => (int?)i.Stock) ?? 0;
        }

        public async Task<int> GetTotalProductsSoldAsync()
        {
            return await _context.DetallesVenta.SumAsync(d => (int?)d.Cantidad) ?? 0;
        }

        public async Task<int> GetLowStockCountAsync()
        {
            return await _context.Inventories.CountAsync(i => i.Stock <= i.MinStock);
        }

        public async Task<IEnumerable<DailySalesDto>> GetDailySalesAsync()
        {
            return await _context.Ventas
                .GroupBy(v => v.Fecha.Date)
                .Select(g => new DailySalesDto
                {
                    Fecha = g.Key,
                    Total = g.Sum(x => x.Total)
                })
                .OrderBy(x => x.Fecha)
                .ToListAsync();
        }

        public async Task<IEnumerable<TopProductsDto>> GetTopProductsAsync()
        {
            return await _context.DetallesVenta
                .GroupBy(d => d.IdProducto)
                .Select(g => new TopProductsDto
                {
                    Nombre = _context.Productos
                        .Where(p => p.IdProducto == g.Key)
                        .Select(p => p.Nombre)
                        .FirstOrDefault() ?? string.Empty,
                    TotalVendido = g.Sum(x => x.Cantidad)
                })
                .OrderByDescending(x => x.TotalVendido)
                .Take(10)
                .ToListAsync();
        }

        public async Task<IEnumerable<SalesByCategoryDto>> GetSalesByCategoryAsync()
        {
            return await _context.DetallesVenta
                .Join(_context.Productos,
                    detalle => detalle.IdProducto,
                    producto => producto.IdProducto,
                    (detalle, producto) => new { detalle, producto })
                .Join(_context.Categorias,
                    joinResult => joinResult.producto.IdCategoria,
                    categoria => categoria.IdCategoria,
                    (joinResult, categoria) => new
                    {
                        Categoria = categoria.Nombre,
                        Venta = joinResult.detalle.Subtotal
                    })
                .GroupBy(x => x.Categoria)
                .Select(g => new SalesByCategoryDto
                {
                    Categoria = g.Key,
                    TotalVentas = g.Sum(x => x.Venta)
                })
                .OrderByDescending(x => x.TotalVentas)
                .ToListAsync();
        }

        public async Task<IEnumerable<InventoryStatusDto>> GetInventoryStatusAsync()
        {
            return await _context.Inventories
                .Include(i => i.Producto)
                .Select(i => new InventoryStatusDto
                {
                    IdProducto = i.IdProducto,
                    Nombre = i.Producto != null ? i.Producto.Nombre : string.Empty,
                    Stock = i.Stock,
                    MinStock = i.MinStock,
                    Status = i.Stock <= i.MinStock ? "Low" : "Good"
                })
                .ToListAsync();
        }
    }
}
