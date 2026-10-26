namespace TiendaBolsos.Core.Modules.Products.DTOs
{
    public class UpdateProductDto
    {
        public string? CodigoProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; } = true;

        public int IdCategoria { get; set; }

        public int IdMarca { get; set; }
    }
}
