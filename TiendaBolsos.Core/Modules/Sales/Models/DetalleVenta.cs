using TiendaBolsos.Core.Modules.Products.Models;

namespace TiendaBolsos.Core.Modules.Sales.Models
{
    public class DetalleVenta
    {
        public int IdDetalleVenta { get; set; }

        public int IdVenta { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public Venta Venta { get; set; } = null!;

        public Product Producto { get; set; } = null!;
    }
}
