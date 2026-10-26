namespace TiendaBolsos.Core.Modules.Products.DTOs
{
    public class ProductResponseDto
    {
        public int IdProducto { get; set; }

        public string? CodigoProducto { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string? Descripcion { get; set; }

        public decimal Precio { get; set; }

        public int Stock { get; set; }

        public string? ImagenUrl { get; set; }

        public bool Activo { get; set; }

        public DateTime FechaRegistro { get; set; }

        public CategoryResponseDto? Categoria { get; set; }

        public int IdCategoria { get; set; }

        public string? Marca { get; set; }

        public int IdMarca { get; set; }
    }
}
