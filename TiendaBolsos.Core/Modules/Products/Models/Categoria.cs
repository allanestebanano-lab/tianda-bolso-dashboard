namespace TiendaBolsos.Core.Modules.Products.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Product>? Productos { get; set; }
    }
}
