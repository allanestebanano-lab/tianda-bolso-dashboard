using InventoryEntity = TiendaBolsos.Core.Modules.Inventory.Models.Inventory;
using InventoryMovementEntity = TiendaBolsos.Core.Modules.Inventory.Models.InventoryMovement;
using TiendaBolsos.Core.Modules.Inventory.DTOs;
using TiendaBolsos.Core.Modules.Inventory.Interfaces;

namespace TiendaBolsos.Core.Modules.Inventory.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _repository;

        public InventoryService(IInventoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllAsync()
        {
            var inventories = await _repository.GetAllAsync();

            return inventories.Select(MapToDto);
        }

        public async Task<InventoryResponseDto?> GetByIdAsync(int id)
        {
            var inventory = await _repository.GetByIdAsync(id);

            return inventory is null ? null : MapToDto(inventory);
        }

        public async Task<InventoryResponseDto> CreateAsync(CreateInventoryDto dto)
        {
            var inventory = new InventoryEntity
            {
                IdProducto = dto.IdProducto,
                Stock = dto.Stock,
                MinStock = dto.MinStock,
                LastUpdate = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(inventory);

            return MapToDto(created);
        }

        public async Task<InventoryResponseDto> UpdateAsync(int id, UpdateInventoryDto dto)
        {
            var inventory = await _repository.GetByIdAsync(id);

            if (inventory is null)
            {
                throw new KeyNotFoundException("Inventario no encontrado");
            }

            inventory.Stock = dto.Stock;
            inventory.MinStock = dto.MinStock;
            inventory.LastUpdate = DateTime.UtcNow;

            var updated = await _repository.UpdateAsync(inventory);

            return MapToDto(updated);
        }

        public async Task<IEnumerable<InventoryMovementDto>> GetMovementsAsync()
        {
            var movements = await _repository.GetMovementsAsync();

            return movements.Select(m => new InventoryMovementDto
            {
                IdMovement = m.IdMovement,
                IdInventory = m.IdInventory,
                Type = m.Type,
                Quantity = m.Quantity,
                Date = m.Date
            });
        }

        public async Task<InventoryMovementDto> CreateMovementAsync(CreateInventoryMovementDto dto)
        {
            var movement = new InventoryMovementEntity
            {
                IdInventory = dto.IdInventory,
                Type = dto.Type,
                Quantity = dto.Quantity,
                Date = DateTime.UtcNow
            };

            var created = await _repository.CreateMovementAsync(movement);

            return new InventoryMovementDto
            {
                IdMovement = created.IdMovement,
                IdInventory = created.IdInventory,
                Type = created.Type,
                Quantity = created.Quantity,
                Date = created.Date
            };
        }

        private static InventoryResponseDto MapToDto(InventoryEntity inventory)
        {
            return new InventoryResponseDto
            {
                IdInventory = inventory.IdInventory,
                IdProducto = inventory.IdProducto,
                Stock = inventory.Stock,
                MinStock = inventory.MinStock,
                LastUpdate = inventory.LastUpdate,
                NombreProducto = inventory.Producto?.Nombre,
                CodigoProducto = inventory.Producto?.CodigoProducto
            };
        }
    }
}
