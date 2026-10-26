namespace TiendaBolsos.Core.Modules.Products.DTOs
{
    public class CategoryResponseDto
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; }
    }
}
