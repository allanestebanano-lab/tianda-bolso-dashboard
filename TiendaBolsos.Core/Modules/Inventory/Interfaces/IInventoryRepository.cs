using InventoryEntity = TiendaBolsos.Core.Modules.Inventory.Models.Inventory;
using InventoryMovementEntity = TiendaBolsos.Core.Modules.Inventory.Models.InventoryMovement;

namespace TiendaBolsos.Core.Modules.Inventory.Interfaces
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<InventoryEntity>> GetAllAsync();

        Task<InventoryEntity?> GetByIdAsync(int id);

        Task<InventoryEntity?> GetByProductIdAsync(int productId);

        Task<InventoryEntity> CreateAsync(InventoryEntity inventory);

        Task<InventoryEntity> UpdateAsync(InventoryEntity inventory);

        Task<IEnumerable<InventoryMovementEntity>> GetMovementsAsync();

        Task<InventoryMovementEntity> CreateMovementAsync(InventoryMovementEntity movement);
    }
}
