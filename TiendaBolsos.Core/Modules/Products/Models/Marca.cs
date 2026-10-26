namespace TiendaBolsos.Core.Modules.Products.Models
{
    public class Marca
    {
        public int IdMarca { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        public ICollection<Product>? Productos { get; set; }
    }
}
