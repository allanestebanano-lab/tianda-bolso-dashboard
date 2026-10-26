using TiendaBolsos.Core.Modules.Inventory.DTOs;

namespace TiendaBolsos.Core.Modules.Inventory.Interfaces
{
    public interface IInventoryService
    {
        Task<IEnumerable<InventoryResponseDto>> GetAllAsync();

        Task<InventoryResponseDto?> GetByIdAsync(int id);

        Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto);

        Task<InventoryResponseDto> UpdateAsync(int id, UpdateInventoryDto dto);

        Task<IEnumerable<InventoryMovementDto>> GetMovementsAsync();

        Task<InventoryMovementDto> CreateMovementAsync(CreateInventoryMovementDto dto);
    }
}
