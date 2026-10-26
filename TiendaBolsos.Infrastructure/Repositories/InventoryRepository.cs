using Microsoft.EntityFrameworkCore;

using TiendaBolsos.Core.Modules.Inventory.Interfaces;
using TiendaBolsos.Core.Modules.Inventory.Models;
using TiendaBolsos.Infrastructure.Data;

namespace TiendaBolsos.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Inventory>> GetAllAsync()
        {
            return await _context.Set<Inventory>()
                .Include(i => i.Producto)
                .ToListAsync();
        }

        public async Task<Inventory?> GetByIdAsync(int id)
        {
            return await _context.Set<Inventory>()
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.IdInventory == id);
        }

        public async Task<Inventory?> GetByProductIdAsync(int productId)
        {
            return await _context.Set<Inventory>()
                .Include(i => i.Producto)
                .FirstOrDefaultAsync(i => i.IdProducto == productId);
        }

        public async Task<Inventory> CreateAsync(Inventory inventory)
        {
            inventory.LastUpdate = DateTime.UtcNow;

            _context.Set<Inventory>().Add(inventory);

            await _context.SaveChangesAsync();

            return inventory;
        }

        public async Task<Inventory> UpdateAsync(Inventory inventory)
        {
            inventory.LastUpdate = DateTime.UtcNow;

            _context.Set<Inventory>().Update(inventory);

            await _context.SaveChangesAsync();

            return inventory;
        }

        public async Task<IEnumerable<InventoryMovement>> GetMovementsAsync()
        {
            return await _context.Set<InventoryMovement>()
                .ToListAsync();
        }

        public async Task<InventoryMovement> CreateMovementAsync(InventoryMovement movement)
        {
            movement.Date = DateTime.UtcNow;

            _context.Set<InventoryMovement>().Add(movement);

            await _context.SaveChangesAsync();

            return movement;
        }
    }
}
