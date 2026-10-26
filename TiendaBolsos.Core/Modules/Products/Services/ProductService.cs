using TiendaBolsos.Core.Modules.Inventory.Interfaces;
using TiendaBolsos.Core.Modules.Products.DTOs;
using TiendaBolsos.Core.Modules.Products.Interfaces;
using TiendaBolsos.Core.Modules.Products.Models;

namespace TiendaBolsos.Core.Modules.Products.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly IInventoryRepository _inventoryRepository;

        public ProductService(
            IProductRepository repository,
            IInventoryRepository inventoryRepository)
        {
            _repository = repository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
        {
            var products = await _repository.GetAllAsync();

            return products.Select(MapToDto);
        }

        public async Task<ProductResponseDto?> GetByIdAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);

            return product is null ? null : MapToDto(product);
        }

        public async Task<ProductResponseDto> CreateAsync(CreateProductDto dto)
        {
            var product = new Product
            {
                CodigoProducto = dto.CodigoProducto,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                Precio = dto.Precio,
                Stock = dto.Stock,
                ImagenUrl = dto.ImagenUrl,
                Activo = dto.Activo,
                IdCategoria = dto.IdCategoria,
                IdMarca = dto.IdMarca,
                FechaRegistro = DateTime.UtcNow
            };

            var created = await _repository.CreateAsync(product);

            var inventory = await _inventoryRepository.GetByProductIdAsync(created.IdProducto);
            if (inventory is null)
            {
                await _inventoryRepository.CreateAsync(new TiendaBolsos.Core.Modules.Inventory.Models.Inventory
                {
                    IdProducto = created.IdProducto,
                    Stock = created.Stock,
                    MinStock = 5,
                    LastUpdate = DateTime.UtcNow
                });
            }

            return MapToDto(created);
        }

        public async Task<ProductResponseDto> UpdateAsync(int id, UpdateProductDto dto)
        {
            var product = await _repository.GetByIdAsync(id);

            if (product is null)
            {
                throw new KeyNotFoundException("Producto no encontrado");
            }

            product.CodigoProducto = dto.CodigoProducto;
            product.Nombre = dto.Nombre;
            product.Descripcion = dto.Descripcion;
            product.Precio = dto.Precio;
            product.Stock = dto.Stock;
            product.ImagenUrl = dto.ImagenUrl;
            product.Activo = dto.Activo;
            product.IdCategoria = dto.IdCategoria;
            product.IdMarca = dto.IdMarca;

            var updated = await _repository.UpdateAsync(product);

            var inventory = await _inventoryRepository.GetByProductIdAsync(updated.IdProducto);
            if (inventory is not null)
            {
                inventory.Stock = updated.Stock;
                inventory.MinStock = inventory.MinStock;
                inventory.LastUpdate = DateTime.UtcNow;
                await _inventoryRepository.UpdateAsync(inventory);
            }
            else
            {
                await _inventoryRepository.CreateAsync(new TiendaBolsos.Core.Modules.Inventory.Models.Inventory
                {
                    IdProducto = updated.IdProducto,
                    Stock = updated.Stock,
                    MinStock = 5,
                    LastUpdate = DateTime.UtcNow
                });
            }

            return MapToDto(updated);
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                IdProducto = product.IdProducto,
                CodigoProducto = product.CodigoProducto,
                Nombre = product.Nombre,
                Descripcion = product.Descripcion,
                Precio = product.Precio,
                Stock = product.Stock,
                ImagenUrl = product.ImagenUrl,
                Activo = product.Activo,
                FechaRegistro = product.FechaRegistro,
                IdCategoria = product.IdCategoria,
                Categoria = product.Categoria is null ? null : new CategoryResponseDto
                {
                    IdCategoria = product.Categoria.IdCategoria,
                    Nombre = product.Categoria.Nombre,
                    FechaRegistro = product.Categoria.FechaRegistro
                },
                IdMarca = product.IdMarca,
                Marca = product.Marca?.Nombre
            };
        }
    }
}
