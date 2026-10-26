namespace TiendaBolsos.Core.Modules.Products.Models
{
    public class Product
    {
        public int IdProducto { get; set; }

        public string? CodigoProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public int IdCategoria { get; set; }

        public int IdMarca { get; set; }

        public Categoria? Categoria { get; set; }

        public Marca? Marca { get; set; }
    }
}
