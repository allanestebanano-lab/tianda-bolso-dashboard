namespace TiendaBolsos.Core.Modules.Auth.Models
{
    public class User
    {
        public int IdUsuario { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string Rol { get; set; } = "Cajero";

        public bool Activo { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}
