using TiendaBolsos.Core.Modules.Auth.Models;

namespace TiendaBolsos.Core.Modules.Sales.Models
{
    public class Venta
    {
        public int IdVenta { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        public decimal Total { get; set; }

        public string? MetodoPago { get; set; }

        public int IdUsuario { get; set; }

        public User Usuario { get; set; } = null!;

        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}
