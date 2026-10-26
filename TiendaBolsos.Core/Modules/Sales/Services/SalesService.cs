using TiendaBolsos.Core.Modules.Inventory.Interfaces;
using TiendaBolsos.Core.Modules.Products.Interfaces;
using TiendaBolsos.Core.Modules.Products.Models;
using TiendaBolsos.Core.Modules.Sales.DTOs;
using TiendaBolsos.Core.Modules.Sales.Interfaces;
using TiendaBolsos.Core.Modules.Sales.Models;

namespace TiendaBolsos.Core.Modules.Sales.Services
{
    public class SalesService : ISalesService
    {
        private readonly ISalesRepository _repository;
        private readonly IProductRepository _productRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public SalesService(
            ISalesRepository repository,
            IProductRepository productRepository,
            IInventoryRepository inventoryRepository)
        {
            _repository = repository;
            _productRepository = productRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<IEnumerable<SaleResponseDto>> GetAllAsync()
        {
            var ventas = await _repository.GetAllAsync();

            return ventas.Select(MapToResponse);
        }

        public async Task<SaleResponseDto?> GetByIdAsync(int id)
        {
            var venta = await _repository.GetByIdAsync(id);

            return venta is null ? null : MapToResponse(venta);
        }

        public async Task<SaleResponseDto> CreateAsync(int userId, CreateSaleDto dto)
        {
            var detalles = new List<DetalleVenta>();
            var productosActualizar = new List<Product>();

            foreach (var item in dto.Detalles)
            {
                var product = await _productRepository.GetByIdAsync(item.IdProducto);

                if (product is null)
                {
                    throw new InvalidOperationException($"Producto con id {item.IdProducto} no encontrado.");
                }

                if (product.Stock < item.Cantidad)
                {
                    throw new InvalidOperationException($"Stock insuficiente para producto '{product.Nombre}'. Stock disponible: {product.Stock}, solicitado: {item.Cantidad}.");
                }

                detalles.Add(new DetalleVenta
                {
                    IdProducto = item.IdProducto,
                    Cantidad = item.Cantidad,
                    PrecioUnitario = product.Precio,
                    Subtotal = product.Precio * item.Cantidad
                });

                // Preparar actualización de stock
                product.Stock -= item.Cantidad;
                productosActualizar.Add(product);
            }

            var venta = new Venta
            {
                Fecha = DateTime.UtcNow,
                Total = detalles.Sum(d => d.Subtotal),
                MetodoPago = dto.MetodoPago,
                IdUsuario = userId,
                Detalles = detalles
            };

            var created = await _repository.CreateAsync(venta);

            // Actualizar stocks después de crear la venta
            foreach (var product in productosActualizar)
            {
                await _productRepository.UpdateAsync(product);

                var inventory = await _inventoryRepository.GetByProductIdAsync(product.IdProducto);

                if (inventory is not null)
                {
                    inventory.Stock = product.Stock;
                    await _inventoryRepository.UpdateAsync(inventory);
                }
                else
                {
                    await _inventoryRepository.CreateAsync(new TiendaBolsos.Core.Modules.Inventory.Models.Inventory
                    {
                        IdProducto = product.IdProducto,
                        Stock = product.Stock,
                        MinStock = 5,
                        LastUpdate = DateTime.UtcNow
                    });
                }
            }

            return MapToResponse(created);
        }

        private static SaleResponseDto MapToResponse(Venta venta)
        {
            return new SaleResponseDto
            {
                IdVenta = venta.IdVenta,
                Fecha = venta.Fecha,
                Total = venta.Total,
                MetodoPago = venta.MetodoPago,
                IdUsuario = venta.IdUsuario,
                Detalles = venta.Detalles?.Select(d => new SaleItemDto
                {
                    IdProducto = d.IdProducto,
                    NombreProducto = d.Producto?.Nombre,
                    Cantidad = d.Cantidad,
                    PrecioUnitario = d.PrecioUnitario,
                    Subtotal = d.Subtotal
                }).ToList() ?? new List<SaleItemDto>()
            };
        }
    }
}
