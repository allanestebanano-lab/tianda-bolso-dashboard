using TiendaBolsos.Core.Modules.Sales.Models;

namespace TiendaBolsos.Core.Modules.Sales.Interfaces
{
    public interface ISalesRepository
    {
        Task<IEnumerable<Venta>> GetAllAsync();

        Task<Venta?> GetByIdAsync(int id);

        Task<Venta> CreateAsync(Venta venta);
    }
}
